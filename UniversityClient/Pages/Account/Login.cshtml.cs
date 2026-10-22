using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniversityClient.Services;

namespace UniversityClient.Pages.Account;

public class LoginModel : PageModel
{
    public const string TokenKey = "AccessToken";
    public const string UserKey = "UserName";

    private readonly UniversityApiClient _api;

    public LoginModel(UniversityApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    [Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var response = await _api.LoginAsync(UserName, Password);
        if (!response.IsSuccess)
        {
            Error = response.Error;
            return Page();
        }

        HttpContext.Session.SetString(TokenKey, response.Data!.AccessToken);
        HttpContext.Session.SetString(UserKey, response.Data.UserName);
        return RedirectToPage("/My");
    }
}
