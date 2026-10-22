using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Users;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/users")]
[Authorize(Policy = Policies.AdminOnly)]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<UserDto>>>> GetAll()
    {
        return ToResponse(await _userService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReturnResult<UserDto>>> GetById(string id)
    {
        return ToResponse(await _userService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ReturnResult<UserDto>>> Create(UserCreateDto dto)
    {
        return ToResponse(await _userService.CreateAsync(dto));
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<ReturnResult<UserDto>>> SetRole(string id, UserRoleUpdateDto dto)
    {
        return ToResponse(await _userService.SetRoleAsync(id, dto));
    }

    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<ReturnResult<UserDto>>> Unlock(string id)
    {
        return ToResponse(await _userService.UnlockAsync(id));
    }
}
