using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : BaseApiController
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [Authorize(Policy = Policies.Staff)]
    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<StudentDto>>>> GetAll()
    {
        return ToResponse(await _studentService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<StudentDto>>> GetById(int id)
    {
        return ToResponse(await _studentService.GetByIdAsync(id));
    }

    [HttpGet("{id:int}/courses")]
    public async Task<ActionResult<ReturnResult<StudentWithCoursesDto>>> GetWithCourses(int id)
    {
        return ToResponse(await _studentService.GetWithCoursesAsync(id));
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Create(StudentCreateDto dto)
    {
        return ToResponse(await _studentService.CreateAsync(dto));
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Update(int id, StudentUpdateDto dto)
    {
        return ToResponse(await _studentService.UpdateAsync(id, dto));
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Delete(int id)
    {
        return ToResponse(await _studentService.DeleteAsync(id));
    }
}
