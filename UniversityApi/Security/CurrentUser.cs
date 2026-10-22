using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace UniversityApi.Security;

public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAdmin { get; }
    bool IsTeacher { get; }
    bool IsStudent { get; }
    int? StudentId { get; }
    int? TeacherId { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    public bool IsAdmin => Principal?.IsInRole(Roles.Admin) ?? false;
    public bool IsTeacher => Principal?.IsInRole(Roles.Teacher) ?? false;
    public bool IsStudent => Principal?.IsInRole(Roles.Student) ?? false;
    public int? StudentId => ReadInt(CustomClaims.StudentId);
    public int? TeacherId => ReadInt(CustomClaims.TeacherId);

    private int? ReadInt(string claimType)
    {
        var value = Principal?.FindFirstValue(claimType);
        return int.TryParse(value, out var id) ? id : null;
    }
}
