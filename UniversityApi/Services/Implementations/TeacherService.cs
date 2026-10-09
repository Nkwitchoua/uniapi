using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ITeacherRepository teacherRepository, IMapper mapper, ILogger<TeacherService> logger)
    {
        _teacherRepository = teacherRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<TeacherDto>>> GetAllAsync()
    {
        var teachers = await _teacherRepository.GetAllAsync();
        return ReturnResult<List<TeacherDto>>.Success(_mapper.Map<List<TeacherDto>>(teachers));
    }

    public async Task<ReturnResult<TeacherDto>> GetByIdAsync(int id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null)
        {
            _logger.LogWarning("Teacher with id {TeacherId} not found", id);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status404NotFound, "TEACHER_NOT_FOUND", "Teacher not found");
        }

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto)
    {
        if (await _teacherRepository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Teacher with email {Email} already exists", dto.Email);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status409Conflict, "TEACHER_EMAIL_EXISTS", "Teacher with this email already exists");
        }

        var teacher = _mapper.Map<Teacher>(dto);

        await _teacherRepository.AddAsync(teacher);
        _logger.LogInformation("Teacher {TeacherId} created", teacher.Id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher), StatusCodes.Status201Created);
    }

    public async Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null)
        {
            _logger.LogWarning("Teacher with id {TeacherId} not found", id);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status404NotFound, "TEACHER_NOT_FOUND", "Teacher not found");
        }

        if (await _teacherRepository.EmailExistsAsync(dto.Email, id))
        {
            _logger.LogWarning("Teacher with email {Email} already exists", dto.Email);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status409Conflict, "TEACHER_EMAIL_EXISTS", "Teacher with this email already exists");
        }

        _mapper.Map(dto, teacher);
        await _teacherRepository.UpdateAsync(teacher);
        _logger.LogInformation("Teacher {TeacherId} updated", id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> DeleteAsync(int id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null)
        {
            _logger.LogWarning("Teacher with id {TeacherId} not found", id);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status404NotFound, "TEACHER_NOT_FOUND", "Teacher not found");
        }

        if (await _teacherRepository.HasCoursesAsync(id))
        {
            _logger.LogWarning("Teacher {TeacherId} cannot be deleted: teacher has courses", id);
            return ReturnResult<TeacherDto>.Fail(StatusCodes.Status409Conflict, "TEACHER_HAS_COURSES", "Teacher has courses and cannot be deleted");
        }

        await _teacherRepository.DeleteAsync(teacher);
        _logger.LogInformation("Teacher {TeacherId} deleted", id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }
}
