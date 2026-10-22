using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;

namespace UniversityApi.Services.Interfaces;

public interface IAuthService
{
    Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto);
    Task<ReturnResult<TokenDto>> LoginAsync(LoginDto dto);
    Task<ReturnResult<UserDto>> GetCurrentUserAsync();
}
