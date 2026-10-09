using UniversityApi.Common;
using UniversityApi.DTO.Students;

namespace UniversityApi.Services.Interfaces;

public interface IStudentService
{
    Task<ReturnResult<List<StudentDto>>> GetAllAsync();
    Task<ReturnResult<StudentDto>> GetByIdAsync(int id);
    Task<ReturnResult<StudentWithCoursesDto>> GetWithCoursesAsync(int id);
    Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto);
    Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto);
    Task<ReturnResult<StudentDto>> DeleteAsync(int id);
}
