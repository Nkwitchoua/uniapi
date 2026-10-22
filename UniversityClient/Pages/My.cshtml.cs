using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniversityClient.Models;
using UniversityClient.Pages.Account;
using UniversityClient.Services;

namespace UniversityClient.Pages;

public class MyModel : PageModel
{
    private readonly UniversityApiClient _api;

    public MyModel(UniversityApiClient api)
    {
        _api = api;
    }

    public UserInfo? Me { get; private set; }
    public List<EnrollmentInfo> Enrollments { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString(LoginModel.TokenKey);
        if (token == null)
            return RedirectToPage("/Account/Login");

        var me = await _api.GetMeAsync(token);
        if (me.StatusCode == StatusCodes.Status401Unauthorized)
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/Login");
        }

        Me = me.Data;
        Error = me.Error;

        if (Me != null)
        {
            var enrollments = await _api.GetEnrollmentsAsync(token);
            Enrollments = enrollments.Data ?? new();
            Error = enrollments.Error;
        }

        return Page();
    }
}
