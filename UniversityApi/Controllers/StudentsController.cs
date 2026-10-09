using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : BaseApiController
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

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

    [HttpPost]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Create(StudentCreateDto dto)
    {
        return ToResponse(await _studentService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Update(int id, StudentUpdateDto dto)
    {
        return ToResponse(await _studentService.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReturnResult<StudentDto>>> Delete(int id)
    {
        return ToResponse(await _studentService.DeleteAsync(id));
    }
}
