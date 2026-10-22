using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Users;

public class UserRoleUpdateDto
{
    [Required]
    public string Role { get; set; } = string.Empty;

    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}
