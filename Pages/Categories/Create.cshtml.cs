using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Categories;

public class CreateModel : PageModel
{
    private readonly ChiTieuContext _context;

    public CreateModel(ChiTieuContext context)
    {
        _context = context;
    }

    [BindProperty]
    public DanhMuc DanhMuc { get; set; } = new()
    {
        TenDanhMuc = string.Empty,
    };

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.DanhMucs.Add(DanhMuc);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
