namespace UniversityApi.DTO.Courses;

public class CourseFilterDto
{
    public string? Name { get; set; }
    public int? TeacherId { get; set; }
    public int? MinCredits { get; set; }
    public int? MaxCredits { get; set; }
}
