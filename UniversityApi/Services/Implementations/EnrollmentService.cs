using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IMapper mapper,
        ICurrentUser currentUser,
        ILogger<EnrollmentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReturnResult<List<EnrollmentDto>>> GetAllAsync()
    {
        List<Enrollment> enrollments;
        if (_currentUser.IsAdmin)
            enrollments = await _enrollmentRepository.GetAllAsync();
        else if (_currentUser.IsTeacher)
            enrollments = _currentUser.TeacherId.HasValue ? await _enrollmentRepository.GetAllAsync(teacherId: _currentUser.TeacherId) : new();
        else
            enrollments = _currentUser.StudentId.HasValue ? await _enrollmentRepository.GetAllAsync(studentId: _currentUser.StudentId) : new();

        return ReturnResult<List<EnrollmentDto>>.Success(_mapper.Map<List<EnrollmentDto>>(enrollments));
    }

    public async Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment with id {EnrollmentId} not found", id);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status404NotFound, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        }

        if (!CanRead(enrollment))
        {
            _logger.LogWarning("User {UserId} denied access to enrollment {EnrollmentId}", _currentUser.UserId, id);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status403Forbidden, "FORBIDDEN", "You can view only your own enrollments");
        }

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> CreateAsync(EnrollmentCreateDto dto)
    {
        if (!await _studentRepository.ExistsAsync(dto.StudentId))
        {
            _logger.LogWarning("Cannot enroll: student with id {StudentId} not found", dto.StudentId);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "Student not found");
        }

        if (!await _courseRepository.ExistsAsync(dto.CourseId))
        {
            _logger.LogWarning("Cannot enroll: course with id {CourseId} not found", dto.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status404NotFound, "COURSE_NOT_FOUND", "Course not found");
        }

        if (await _enrollmentRepository.ExistsAsync(dto.StudentId, dto.CourseId))
        {
            _logger.LogWarning("Student {StudentId} is already enrolled in course {CourseId}", dto.StudentId, dto.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status409Conflict, "ALREADY_ENROLLED", "Student is already enrolled in this course");
        }

        var enrollment = _mapper.Map<Enrollment>(dto);
        enrollment.EnrollmentDate = DateTime.UtcNow;

        await _enrollmentRepository.AddAsync(enrollment);
        _logger.LogInformation("Enrollment {EnrollmentId} created: student {StudentId} enrolled in course {CourseId}", enrollment.Id, dto.StudentId, dto.CourseId);

        var created = await _enrollmentRepository.GetByIdAsync(enrollment.Id);
        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(created), StatusCodes.Status201Created);
    }

    public async Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateDto dto)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment with id {EnrollmentId} not found", id);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status404NotFound, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        }

        if (!_currentUser.IsAdmin && !IsOwnCourse(enrollment))
        {
            _logger.LogWarning("Teacher {TeacherId} tried to change grade on course {CourseId} of another teacher", _currentUser.TeacherId, enrollment.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status403Forbidden, "FORBIDDEN", "You can change grades only on your own courses");
        }

        _mapper.Map(dto, enrollment);
        await _enrollmentRepository.UpdateAsync(enrollment);
        _logger.LogInformation("Enrollment {EnrollmentId} updated: grade set to {Grade}", id, dto.Grade);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> DeleteAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment with id {EnrollmentId} not found", id);
            return ReturnResult<EnrollmentDto>.Fail(StatusCodes.Status404NotFound, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        }

        await _enrollmentRepository.DeleteAsync(enrollment);
        _logger.LogInformation("Enrollment {EnrollmentId} deleted: student {StudentId} removed from course {CourseId}", id, enrollment.StudentId, enrollment.CourseId);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    private bool CanRead(Enrollment enrollment)
    {
        if (_currentUser.IsAdmin)
            return true;

        if (_currentUser.IsTeacher)
            return IsOwnCourse(enrollment);

        return _currentUser.StudentId.HasValue && enrollment.StudentId == _currentUser.StudentId;
    }

    private bool IsOwnCourse(Enrollment enrollment) =>
        _currentUser.IsTeacher && _currentUser.TeacherId.HasValue && enrollment.Course.TeacherId == _currentUser.TeacherId;
}
