using UniversityApi.Common;
using UniversityApi.DTO.Courses;

namespace UniversityApi.Services.Interfaces;

public interface ICourseService
{
    Task<ReturnResult<List<TDto>>> GetAllAsync<TDto>(CourseFilterDto filter);
    Task<ReturnResult<TDto>> GetByIdAsync<TDto>(int id);
    Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto);
    Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto);
    Task<ReturnResult<CourseDto>> DeleteAsync(int id);
}
