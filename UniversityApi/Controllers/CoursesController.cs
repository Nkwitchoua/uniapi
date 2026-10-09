using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<CourseDto>>>> GetAll([FromQuery] CourseFilterDto filter)
    {
        return ToResponse(await _courseService.GetAllAsync(filter));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<CourseDto>>> GetById(int id)
    {
        return ToResponse(await _courseService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ReturnResult<CourseDto>>> Create(CourseCreateDto dto)
    {
        return ToResponse(await _courseService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnResult<CourseDto>>> Update(int id, CourseUpdateDto dto)
    {
        return ToResponse(await _courseService.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReturnResult<CourseDto>>> Delete(int id)
    {
        return ToResponse(await _courseService.DeleteAsync(id));
    }
}
