using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniversityClient.Models;
using UniversityClient.Services;

namespace UniversityClient.Pages;

public class IndexModel : PageModel
{
    private readonly UniversityApiClient _api;

    public IndexModel(UniversityApiClient api)
    {
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public int V { get; set; } = 1;

    public List<CourseV1> CoursesV1 { get; private set; } = new();
    public List<CourseV2> CoursesV2 { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        if (V == 2)
        {
            var response = await _api.GetCoursesV2Async();
            CoursesV2 = response.Data ?? new();
            Error = response.Error;
        }
        else
        {
            V = 1;
            var response = await _api.GetCoursesV1Async();
            CoursesV1 = response.Data ?? new();
            Error = response.Error;
        }
    }
}
