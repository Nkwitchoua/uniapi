namespace UniversityApi.DTO.Users;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
    public bool IsLockedOut { get; set; }
}
