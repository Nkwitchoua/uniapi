using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Security;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : BaseApiController
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ReturnResult<List<EnrollmentDto>>>> GetAll()
    {
        return ToResponse(await _enrollmentService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnResult<EnrollmentDto>>> GetById(int id)
    {
        return ToResponse(await _enrollmentService.GetByIdAsync(id));
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ReturnResult<EnrollmentDto>>> Create(EnrollmentCreateDto dto)
    {
        return ToResponse(await _enrollmentService.CreateAsync(dto));
    }

    [Authorize(Policy = Policies.CanGrade)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReturnResult<EnrollmentDto>>> UpdateGrade(int id, EnrollmentUpdateDto dto)
    {
        return ToResponse(await _enrollmentService.UpdateGradeAsync(id, dto));
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ReturnResult<EnrollmentDto>>> Delete(int id)
    {
        return ToResponse(await _enrollmentService.DeleteAsync(id));
    }
}
