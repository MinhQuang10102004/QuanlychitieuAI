using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Households;

public class CreateModel : PageModel
{
    private readonly HouseholdService _householdService;

    public CreateModel(HouseholdService householdService)
    {
        _householdService = householdService;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        if (string.IsNullOrWhiteSpace(Name))
        {
            ModelState.AddModelError(nameof(Name), "Vui lòng nhập tên hộ gia đình.");
            return Page();
        }

        var hh = await _householdService.CreateHouseholdAsync(userId.Value, Name);
        if (hh == null)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo hộ gia đình. Vui lòng thử lại.");
            return Page();
        }

        return RedirectToPage("/Households/Index");
    }
}
