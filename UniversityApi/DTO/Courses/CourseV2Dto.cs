namespace UniversityApi.DTO.Courses;

public class CourseV2Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public CourseTeacherDto Teacher { get; set; } = new();
    public int EnrolledStudentsCount { get; set; }
}

public class CourseTeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
