using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository studentRepository, IMapper mapper, ICurrentUser currentUser, ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReturnResult<List<StudentDto>>> GetAllAsync()
    {
        var students = await _studentRepository.GetAllAsync();
        return ReturnResult<List<StudentDto>>.Success(_mapper.Map<List<StudentDto>>(students));
    }

    public async Task<ReturnResult<StudentDto>> GetByIdAsync(int id)
    {
        if (IsOtherStudent(id))
            return Forbidden<StudentDto>(id);

        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            _logger.LogWarning("Student with id {StudentId} not found", id);
            return ReturnResult<StudentDto>.Fail(StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "Student not found");
        }

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentWithCoursesDto>> GetWithCoursesAsync(int id)
    {
        if (IsOtherStudent(id))
            return Forbidden<StudentWithCoursesDto>(id);

        var student = await _studentRepository.GetWithCoursesAsync(id);
        if (student == null)
        {
            _logger.LogWarning("Student with id {StudentId} not found", id);
            return ReturnResult<StudentWithCoursesDto>.Fail(StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "Student not found");
        }

        if (_currentUser.IsTeacher && !_currentUser.IsAdmin)
            student.Enrollments = student.Enrollments.Where(e => e.Course.TeacherId == _currentUser.TeacherId).ToList();

        return ReturnResult<StudentWithCoursesDto>.Success(_mapper.Map<StudentWithCoursesDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto)
    {
        if (await _studentRepository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Student with email {Email} already exists", dto.Email);
            return ReturnResult<StudentDto>.Fail(StatusCodes.Status409Conflict, "STUDENT_EMAIL_EXISTS", "Student with this email already exists");
        }

        var student = _mapper.Map<Student>(dto);
        student.CreatedAt = DateTime.UtcNow;

        await _studentRepository.AddAsync(student);
        _logger.LogInformation("Student {StudentId} created", student.Id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student), StatusCodes.Status201Created);
    }

    public async Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            _logger.LogWarning("Student with id {StudentId} not found", id);
            return ReturnResult<StudentDto>.Fail(StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "Student not found");
        }

        if (await _studentRepository.EmailExistsAsync(dto.Email, id))
        {
            _logger.LogWarning("Student with email {Email} already exists", dto.Email);
            return ReturnResult<StudentDto>.Fail(StatusCodes.Status409Conflict, "STUDENT_EMAIL_EXISTS", "Student with this email already exists");
        }

        _mapper.Map(dto, student);
        await _studentRepository.UpdateAsync(student);
        _logger.LogInformation("Student {StudentId} updated", id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> DeleteAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            _logger.LogWarning("Student with id {StudentId} not found", id);
            return ReturnResult<StudentDto>.Fail(StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "Student not found");
        }

        await _studentRepository.DeleteAsync(student);
        _logger.LogInformation("Student {StudentId} deleted", id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    private bool IsOtherStudent(int id) =>
        !_currentUser.IsAdmin && !_currentUser.IsTeacher && _currentUser.StudentId != id;

    private ReturnResult<T> Forbidden<T>(int id)
    {
        _logger.LogWarning("User {UserId} denied access to student {StudentId}", _currentUser.UserId, id);
        return ReturnResult<T>.Fail(StatusCodes.Status403Forbidden, "FORBIDDEN", "You can view only your own data");
    }
}
