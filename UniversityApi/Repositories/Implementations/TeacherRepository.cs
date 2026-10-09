using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class TeacherRepository : ITeacherRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Teacher>> GetAllAsync()
    {
        return await _context.Teachers.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
    }

    public async Task<Teacher?> GetByIdAsync(int id)
    {
        return await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Teachers.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Teachers.AnyAsync(t => t.Email == email && t.Id != excludeId);
    }

    public async Task<bool> HasCoursesAsync(int id)
    {
        return await _context.Courses.AnyAsync(c => c.TeacherId == id);
    }

    public async Task AddAsync(Teacher teacher)
    {
        await _context.Teachers.AddAsync(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Teacher teacher)
    {
        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
    }
}
