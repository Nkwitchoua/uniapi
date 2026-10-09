using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ITeacherRepository
{
    Task<List<Teacher>> GetAllAsync();
    Task<Teacher?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> HasCoursesAsync(int id);
    Task AddAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(Teacher teacher);
}
