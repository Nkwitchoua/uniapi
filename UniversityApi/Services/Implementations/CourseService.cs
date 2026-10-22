using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courseRepository, ITeacherRepository teacherRepository, IMapper mapper, ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _teacherRepository = teacherRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<TDto>>> GetAllAsync<TDto>(CourseFilterDto filter)
    {
        var courses = await _courseRepository.GetAllAsync(filter.Name, filter.TeacherId, filter.MinCredits, filter.MaxCredits);
        return ReturnResult<List<TDto>>.Success(_mapper.Map<List<TDto>>(courses));
    }

    public async Task<ReturnResult<TDto>> GetByIdAsync<TDto>(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Course with id {CourseId} not found", id);
            return ReturnResult<TDto>.Fail(StatusCodes.Status404NotFound, "COURSE_NOT_FOUND", "Course not found");
        }

        return ReturnResult<TDto>.Success(_mapper.Map<TDto>(course));
    }

    public async Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto)
    {
        if (!await _teacherRepository.ExistsAsync(dto.TeacherId))
        {
            _logger.LogWarning("Cannot create course: teacher with id {TeacherId} not found", dto.TeacherId);
            return ReturnResult<CourseDto>.Fail(StatusCodes.Status404NotFound, "TEACHER_NOT_FOUND", "Teacher not found");
        }

        var course = _mapper.Map<Course>(dto);
        course.CreatedAt = DateTime.UtcNow;

        await _courseRepository.AddAsync(course);
        _logger.LogInformation("Course {CourseId} created", course.Id);

        var created = await _courseRepository.GetByIdAsync(course.Id);
        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(created), StatusCodes.Status201Created);
    }

    public async Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Course with id {CourseId} not found", id);
            return ReturnResult<CourseDto>.Fail(StatusCodes.Status404NotFound, "COURSE_NOT_FOUND", "Course not found");
        }

        if (!await _teacherRepository.ExistsAsync(dto.TeacherId))
        {
            _logger.LogWarning("Cannot update course: teacher with id {TeacherId} not found", dto.TeacherId);
            return ReturnResult<CourseDto>.Fail(StatusCodes.Status404NotFound, "TEACHER_NOT_FOUND", "Teacher not found");
        }

        _mapper.Map(dto, course);
        await _courseRepository.UpdateAsync(course);
        _logger.LogInformation("Course {CourseId} updated", id);

        var updated = await _courseRepository.GetByIdAsync(id);
        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(updated));
    }

    public async Task<ReturnResult<CourseDto>> DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Course with id {CourseId} not found", id);
            return ReturnResult<CourseDto>.Fail(StatusCodes.Status404NotFound, "COURSE_NOT_FOUND", "Course not found");
        }

        await _courseRepository.DeleteAsync(course);
        _logger.LogInformation("Course {CourseId} deleted", id);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }
}
