using UniversityApi.Models;

namespace UniversityApi.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Teachers.Any())
            return;

        var now = DateTime.UtcNow;

        var ivanov = new Teacher { FirstName = "Ivan", LastName = "Ivanov", Email = "ivanov@university.kz", Department = "Software Engineering" };
        var petrova = new Teacher { FirstName = "Anna", LastName = "Petrova", Email = "petrova@university.kz", Department = "Mathematics" };
        var sidorov = new Teacher { FirstName = "Oleg", LastName = "Sidorov", Email = "sidorov@university.kz", Department = "Information Systems" };

        var webDev = new Course { Name = "Web Services Development", Description = "REST API with ASP.NET Core", Credits = 5, Teacher = ivanov, CreatedAt = now };
        var databases = new Course { Name = "Databases", Description = "SQL and PostgreSQL", Credits = 4, Teacher = sidorov, CreatedAt = now };
        var algebra = new Course { Name = "Linear Algebra", Description = "Matrices and vectors", Credits = 3, Teacher = petrova, CreatedAt = now };
        var calculus = new Course { Name = "Calculus", Description = "Limits, derivatives and integrals", Credits = 6, Teacher = petrova, CreatedAt = now };

        var alex = new Student { FirstName = "Alex", LastName = "Smith", Email = "alex.smith@student.kz", BirthDate = new DateOnly(2004, 3, 15), CreatedAt = now };
        var maria = new Student { FirstName = "Maria", LastName = "Kim", Email = "maria.kim@student.kz", BirthDate = new DateOnly(2003, 11, 2), CreatedAt = now };
        var daniyar = new Student { FirstName = "Daniyar", LastName = "Akhmetov", Email = "daniyar.akhmetov@student.kz", BirthDate = new DateOnly(2005, 6, 20), CreatedAt = now };
        var elena = new Student { FirstName = "Elena", LastName = "Popova", Email = "elena.popova@student.kz", BirthDate = new DateOnly(2004, 1, 9), CreatedAt = now };

        context.Enrollments.AddRange(
            new Enrollment { Student = alex, Course = webDev, EnrollmentDate = now, Grade = 90 },
            new Enrollment { Student = alex, Course = databases, EnrollmentDate = now, Grade = 85 },
            new Enrollment { Student = maria, Course = webDev, EnrollmentDate = now, Grade = 78 },
            new Enrollment { Student = maria, Course = algebra, EnrollmentDate = now },
            new Enrollment { Student = daniyar, Course = calculus, EnrollmentDate = now, Grade = 65 });

        context.Students.Add(elena);
        context.SaveChanges();
    }
}
