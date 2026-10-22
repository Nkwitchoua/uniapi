using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Users;

public class UserCreateDto
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}
