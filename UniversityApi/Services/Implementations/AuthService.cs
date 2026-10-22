using Microsoft.AspNetCore.Identity;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IStudentRepository _studentRepository;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IUserService userService,
        IStudentRepository studentRepository,
        ApplicationDbContext context,
        ICurrentUser currentUser,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userService = userService;
        _studentRepository = studentRepository;
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
        {
            _logger.LogWarning("Registration rejected: user name {UserName} is taken", dto.UserName);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status409Conflict, "USER_ALREADY_EXISTS", "User with this user name already exists");
        }

        if (await _studentRepository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Registration rejected: student email {Email} already exists", dto.Email);
            return ReturnResult<UserDto>.Fail(StatusCodes.Status409Conflict, "STUDENT_EMAIL_EXISTS", "Student with this email already exists");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            BirthDate = dto.BirthDate,
            CreatedAt = DateTime.UtcNow
        };
        await _studentRepository.AddAsync(student);

        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email, StudentId = student.Id };

        var created = await _userManager.CreateAsync(user, dto.Password);
        if (!created.Succeeded)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning("Registration of {UserName} failed: {Errors}", dto.UserName, string.Join(",", created.Errors.Select(e => e.Code)));
            return UserService.IdentityFail(created);
        }

        await _userManager.AddToRoleAsync(user, Roles.Student);
        await transaction.CommitAsync();

        _logger.LogInformation("User {UserId} registered as Student {StudentId}", user.Id, student.Id);

        var result = await _userService.GetByIdAsync(user.Id);
        result.StatusCode = StatusCodes.Status201Created;
        return result;
    }

    public async Task<ReturnResult<TokenDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            _logger.LogWarning("Failed login: unknown user {UserName}", dto.UserName);
            return InvalidCredentials();
        }

        var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (signIn.IsLockedOut)
        {
            _logger.LogWarning("Failed login: user {UserId} is locked out", user.Id);
            return ReturnResult<TokenDto>.Fail(StatusCodes.Status401Unauthorized, "ACCOUNT_LOCKED", "Account is temporarily locked due to multiple failed login attempts");
        }

        if (!signIn.Succeeded)
        {
            _logger.LogWarning("Failed login: wrong password for user {UserId}", user.Id);
            return InvalidCredentials();
        }

        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User {UserId} logged in", user.Id);

        return ReturnResult<TokenDto>.Success(_tokenService.CreateAccessToken(user, roles));
    }

    public async Task<ReturnResult<UserDto>> GetCurrentUserAsync()
    {
        if (_currentUser.UserId == null)
            return ReturnResult<UserDto>.Fail(StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required");

        return await _userService.GetByIdAsync(_currentUser.UserId);
    }

    private static ReturnResult<TokenDto> InvalidCredentials() =>
        ReturnResult<TokenDto>.Fail(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Invalid user name or password");
}
