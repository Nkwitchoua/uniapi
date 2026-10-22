using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;
using UniversityApi.Security;

namespace UniversityApi.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await CreateUserAsync(userManager, "admin", "admin@university.kz", "Admin123!", Roles.Admin, null, null);

        var ivanov = await context.Teachers.FirstOrDefaultAsync(t => t.Email == "ivanov@university.kz");
        var petrova = await context.Teachers.FirstOrDefaultAsync(t => t.Email == "petrova@university.kz");
        var alex = await context.Students.FirstOrDefaultAsync(s => s.Email == "alex.smith@student.kz");
        var maria = await context.Students.FirstOrDefaultAsync(s => s.Email == "maria.kim@student.kz");

        await CreateUserAsync(userManager, "ivanov", "ivanov@university.kz", "Teacher123!", Roles.Teacher, null, ivanov?.Id);
        await CreateUserAsync(userManager, "petrova", "petrova@university.kz", "Teacher123!", Roles.Teacher, null, petrova?.Id);
        await CreateUserAsync(userManager, "alex", "alex.smith@student.kz", "Student123!", Roles.Student, alex?.Id, null);
        await CreateUserAsync(userManager, "maria", "maria.kim@student.kz", "Student123!", Roles.Student, maria?.Id, null);
    }

    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager, string userName, string email, string password, string role, int? studentId, int? teacherId)
    {
        if (await userManager.FindByNameAsync(userName) != null)
            return;

        var user = new ApplicationUser { UserName = userName, Email = email, StudentId = studentId, TeacherId = teacherId };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}
