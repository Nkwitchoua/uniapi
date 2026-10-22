using Microsoft.AspNetCore.Identity;

namespace UniversityApi.Models;

public class ApplicationUser : IdentityUser
{
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }

    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
