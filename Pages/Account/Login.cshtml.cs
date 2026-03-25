using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ChiTieuContext _context;
    private readonly IPasswordHasher<NguoiDung> _passwordHasher;

    public LoginModel(ChiTieuContext context, IPasswordHasher<NguoiDung> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.HoTen == Input.HoTen);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Họ và tên hoặc mật khẩu không đúng.");
            return Page();
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, Input.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Họ và tên hoặc mật khẩu không đúng.");
            return Page();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MatKhau = _passwordHasher.HashPassword(user, Input.Password);
            await _context.SaveChangesAsync();
        }

        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
        HttpContext.Session.SetString("UserName", user.HoTen);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToPage("/Index");
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
