using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/courses")]
[Route("api/courses")]
[Authorize(Policy = Policies.AdminOnly)]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<CourseDto>>>> GetAll([FromQuery] CourseFilterDto filter)
    {
        return ToResponse(await _courseService.GetAllAsync<CourseDto>(filter));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<CourseDto>>> GetById(int id)
    {
        return ToResponse(await _courseService.GetByIdAsync<CourseDto>(id));
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
