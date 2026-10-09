using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetWithCoursesAsync(int id)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Students.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Students.AnyAsync(s => s.Email == email && s.Id != excludeId);
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }
}
