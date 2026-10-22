using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.DTO.Users;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReturnResult<List<UserDto>>> GetAllAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();

        var result = new List<UserDto>();
        foreach (var user in users)
            result.Add(await ToDtoAsync(user));

        return ReturnResult<List<UserDto>>.Success(result);
    }

    public async Task<ReturnResult<UserDto>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with id {UserId} not found", id);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "User not found");
        }

        return ReturnResult<UserDto>.Success(await ToDtoAsync(user));
    }

    public async Task<ReturnResult<UserDto>> CreateAsync(UserCreateDto dto)
    {
        var linkError = await ValidateRoleLinkAsync(dto.Role, dto.StudentId, dto.TeacherId, null);
        if (linkError != null)
            return linkError;

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            StudentId = dto.Role == Roles.Student ? dto.StudentId : null,
            TeacherId = dto.Role == Roles.Teacher ? dto.TeacherId : null
        };

        var created = await _userManager.CreateAsync(user, dto.Password);
        if (!created.Succeeded)
            return IdentityFail(created);

        await _userManager.AddToRoleAsync(user, dto.Role);
        _logger.LogInformation("User {UserId} created by admin with role {Role}", user.Id, dto.Role);

        return ReturnResult<UserDto>.Success(await ToDtoAsync(user), StatusCodes.Status201Created);
    }

    public async Task<ReturnResult<UserDto>> SetRoleAsync(string id, UserRoleUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with id {UserId} not found", id);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "User not found");
        }

        if (user.Id == _currentUser.UserId)
        {
            _logger.LogWarning("Admin {UserId} tried to change own role", user.Id);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status400BadRequest, "CANNOT_CHANGE_OWN_ROLE", "You cannot change your own role");
        }

        var linkError = await ValidateRoleLinkAsync(dto.Role, dto.StudentId, dto.TeacherId, user.Id);
        if (linkError != null)
            return linkError;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, dto.Role);

        user.StudentId = dto.Role == Roles.Student ? dto.StudentId : null;
        user.TeacherId = dto.Role == Roles.Teacher ? dto.TeacherId : null;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        _logger.LogInformation("User {UserId} role changed from {OldRoles} to {Role}", user.Id, string.Join(",", currentRoles), dto.Role);

        return ReturnResult<UserDto>.Success(await ToDtoAsync(user));
    }

    public async Task<ReturnResult<UserDto>> UnlockAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with id {UserId} not found", id);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "User not found");
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        _logger.LogInformation("User {UserId} unlocked", user.Id);

        return ReturnResult<UserDto>.Success(await ToDtoAsync(user));
    }

    public async Task<UserDto> ToDtoAsync(ApplicationUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName!,
        Email = user.Email ?? string.Empty,
        Roles = (await _userManager.GetRolesAsync(user)).ToList(),
        StudentId = user.StudentId,
        TeacherId = user.TeacherId,
        IsLockedOut = await _userManager.IsLockedOutAsync(user)
    };

    private async Task<ReturnResult<UserDto>?> ValidateRoleLinkAsync(string role, int? studentId, int? teacherId, string? userId)
    {
        if (!Roles.All.Contains(role))
            return ReturnResult<UserDto>.Fail(StatusCodes.Status400BadRequest, "ROLE_NOT_FOUND", $"Role must be one of: {string.Join(", ", Roles.All)}");

        if (role == Roles.Student)
        {
            if (studentId == null || !await _studentRepository.ExistsAsync(studentId.Value))
                return ReturnResult<UserDto>.Fail(StatusCodes.Status400BadRequest, "STUDENT_LINK_REQUIRED", "Student role requires an existing studentId");

            if (await _userManager.Users.AnyAsync(u => u.StudentId == studentId && u.Id != userId))
                return ReturnResult<UserDto>.Fail(StatusCodes.Status409Conflict, "STUDENT_ALREADY_LINKED", "This student is already linked to another account");
        }

        if (role == Roles.Teacher)
        {
            if (teacherId == null || !await _teacherRepository.ExistsAsync(teacherId.Value))
                return ReturnResult<UserDto>.Fail(StatusCodes.Status400BadRequest, "TEACHER_LINK_REQUIRED", "Teacher role requires an existing teacherId");

            if (await _userManager.Users.AnyAsync(u => u.TeacherId == teacherId && u.Id != userId))
                return ReturnResult<UserDto>.Fail(StatusCodes.Status409Conflict, "TEACHER_ALREADY_LINKED", "This teacher is already linked to another account");
        }

        return null;
    }

    public static ReturnResult<UserDto> IdentityFail(IdentityResult result)
    {
        var duplicate = result.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateUserName) or nameof(IdentityErrorDescriber.DuplicateEmail));
        var message = string.Join(" ", result.Errors.Select(e => e.Description));

        return duplicate
            ? ReturnResult<UserDto>.Fail(StatusCodes.Status409Conflict, "USER_ALREADY_EXISTS", message)
            : ReturnResult<UserDto>.Fail(StatusCodes.Status400BadRequest, "IDENTITY_ERROR", message);
    }
}
