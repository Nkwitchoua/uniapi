using UniversityApi.DTO.Auth;
using UniversityApi.Models;

namespace UniversityApi.Services.Interfaces;

public interface ITokenService
{
    TokenDto CreateAccessToken(ApplicationUser user, IList<string> roles);
}
