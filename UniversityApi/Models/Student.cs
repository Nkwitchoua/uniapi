using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Student
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}
