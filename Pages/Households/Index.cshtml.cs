using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Households;

public class IndexModel : PageModel
{
    private readonly HouseholdService _householdService;

    public IndexModel(HouseholdService householdService)
    {
        _householdService = householdService;
    }

    public HoGiaDinh? CurrentHousehold { get; private set; }

    public async Task<IActionResult> OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentHousehold = await _householdService.GetUserHouseholdAsync(userId.Value);
        return Page();
    }
}
