using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;

namespace UniversityApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ReturnResult<List<EnrollmentDto>>> GetAllAsync();
    Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id);
    Task<ReturnResult<EnrollmentDto>> CreateAsync(EnrollmentCreateDto dto);
    Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateDto dto);
    Task<ReturnResult<EnrollmentDto>> DeleteAsync(int id);
}
