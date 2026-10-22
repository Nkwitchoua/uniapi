using UniversityApi.Common;
using UniversityApi.DTO.Users;

namespace UniversityApi.Services.Interfaces;

public interface IUserService
{
    Task<ReturnResult<List<UserDto>>> GetAllAsync();
    Task<ReturnResult<UserDto>> GetByIdAsync(string id);
    Task<ReturnResult<UserDto>> CreateAsync(UserCreateDto dto);
    Task<ReturnResult<UserDto>> SetRoleAsync(string id, UserRoleUpdateDto dto);
    Task<ReturnResult<UserDto>> UnlockAsync(string id);
}
