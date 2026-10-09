namespace UniversityApi.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public List<Enrollment> Enrollments { get; set; } = new();
}
