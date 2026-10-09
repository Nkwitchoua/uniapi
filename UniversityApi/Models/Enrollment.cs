namespace UniversityApi.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int? Grade { get; set; }

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
