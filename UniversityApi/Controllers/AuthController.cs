using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ReturnResult<UserDto>>> Register(RegisterDto dto)
    {
        return ToResponse(await _authService.RegisterAsync(dto));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ReturnResult<TokenDto>>> Login(LoginDto dto)
    {
        return ToResponse(await _authService.LoginAsync(dto));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ReturnResult<UserDto>>> Me()
    {
        return ToResponse(await _authService.GetCurrentUserAsync());
    }
}
