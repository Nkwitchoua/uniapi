using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniversityClient.Models;
using UniversityClient.Services;

namespace UniversityClient.Pages.Courses;

public class DetailsModel : PageModel
{
    private readonly UniversityApiClient _api;

    public DetailsModel(UniversityApiClient api)
    {
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public int V { get; set; } = 1;

    public CourseV1? CourseV1 { get; private set; }
    public CourseV2? CourseV2 { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync(int id)
    {
        if (V == 2)
        {
            var response = await _api.GetCourseV2Async(id);
            CourseV2 = response.Data;
            Error = response.Error;
        }
        else
        {
            V = 1;
            var response = await _api.GetCourseV1Async(id);
            CourseV1 = response.Data;
            Error = response.Error;
        }
    }
}
