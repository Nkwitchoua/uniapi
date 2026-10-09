using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> ExistsAsync(int studentId, int courseId)
    {
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
    }
}
