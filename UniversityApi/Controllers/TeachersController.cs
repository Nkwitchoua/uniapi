using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize(Policy = Policies.AdminOnly)]
public class TeachersController : BaseApiController
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<TeacherDto>>>> GetAll()
    {
        return ToResponse(await _teacherService.GetAllAsync());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<TeacherDto>>> GetById(int id)
    {
        return ToResponse(await _teacherService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ReturnResult<TeacherDto>>> Create(TeacherCreateDto dto)
    {
        return ToResponse(await _teacherService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnResult<TeacherDto>>> Update(int id, TeacherUpdateDto dto)
    {
        return ToResponse(await _teacherService.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReturnResult<TeacherDto>>> Delete(int id)
    {
        return ToResponse(await _teacherService.DeleteAsync(id));
    }
}
