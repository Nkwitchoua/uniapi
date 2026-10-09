using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync(string? name, int? teacherId, int? minCredits, int? maxCredits);
    Task<Course?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Course course);
}
