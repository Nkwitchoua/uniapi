namespace UniversityApi.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly string[] All = { Admin, Teacher, Student };
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string Staff = "Staff";
    public const string CanGrade = "CanGrade";
}

public static class CustomClaims
{
    public const string Role = "role";
    public const string StudentId = "student_id";
    public const string TeacherId = "teacher_id";
}
