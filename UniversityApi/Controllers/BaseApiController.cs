using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ReturnResult<T>> ToResponse<T>(ReturnResult<T> result)
    {
        result.TraceId = HttpContext.TraceIdentifier;
        return StatusCode(result.StatusCode, result);
    }
}
