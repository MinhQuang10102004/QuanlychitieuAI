using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Categories;

public class DeleteModel : PageModel
{
    private readonly ChiTieuContext _context;

    public DeleteModel(ChiTieuContext context)
    {
        _context = context;
    }

    [BindProperty]
    public DanhMuc? DanhMuc { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        DanhMuc = await _context.DanhMucs
            .AsNoTracking()
            .FirstOrDefaultAsync(dm => dm.MaDanhMuc == id);

        if (DanhMuc == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        if (DanhMuc == null)
        {
            return RedirectToPage("Index");
        }

        var toDelete = await _context.DanhMucs.FindAsync(DanhMuc.MaDanhMuc);
        if (toDelete == null)
        {
            return NotFound();
        }

        _context.DanhMucs.Remove(toDelete);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
