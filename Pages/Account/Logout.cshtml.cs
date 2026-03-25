using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuanLyChiTieu.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("Login");
    }

    public IActionResult OnPost()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("Login");
    }
}
