namespace UniversityApi.DTO.Students;

public class StudentWithCoursesDto : StudentDto
{
    public List<StudentCourseDto> Courses { get; set; } = new();
}
