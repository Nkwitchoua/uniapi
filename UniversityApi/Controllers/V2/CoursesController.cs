using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/courses")]
[AllowAnonymous]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<CourseV2Dto>>>> GetAll([FromQuery] CourseFilterDto filter)
    {
        return ToResponse(await _courseService.GetAllAsync<CourseV2Dto>(filter));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<CourseV2Dto>>> GetById(int id)
    {
        return ToResponse(await _courseService.GetByIdAsync<CourseV2Dto>(id));
    }
}
