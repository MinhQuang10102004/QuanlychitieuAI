using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Categories;

public class EditModel : PageModel
{
    private readonly ChiTieuContext _context;

    public EditModel(ChiTieuContext context)
    {
        _context = context;
    }

    [BindProperty]
    public DanhMuc DanhMuc { get; set; } = new()
    {
        TenDanhMuc = string.Empty,
    };

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var danhMuc = await _context.DanhMucs.FindAsync(id);

        if (danhMuc == null)
        {
            return NotFound();
        }

        DanhMuc = danhMuc;
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

        _context.Attach(DanhMuc).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.DanhMucs.AnyAsync(dm => dm.MaDanhMuc == DanhMuc.MaDanhMuc))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToPage("Index");
    }
}
