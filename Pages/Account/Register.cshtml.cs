using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ChiTieuContext _context;
    private readonly IPasswordHasher<NguoiDung> _passwordHasher;
    private readonly InvitationService _invitationService;

    public RegisterModel(ChiTieuContext context, IPasswordHasher<NguoiDung> passwordHasher, InvitationService invitationService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _invitationService = invitationService;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

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

        var exists = await _context.NguoiDungs.AnyAsync(u => u.Email == Input.Email);
        if (exists)
        {
            ModelState.AddModelError(nameof(Input.Email), "Email đã được sử dụng.");
            return Page();
        }

        var user = new NguoiDung
        {
            HoTen = Input.HoTen,
            Email = Input.Email,
            MatKhau = string.Empty
        };

        // set account type and phone if provided
        user.AccountType = Input.AccountType ?? "Individual";
        if (!string.IsNullOrWhiteSpace(Input.Phone)) user.Phone = Input.Phone;

        user.MatKhau = _passwordHasher.HashPassword(user, Input.Password);

        _context.NguoiDungs.Add(user);
        await _context.SaveChangesAsync();

        // if there is an invite token, accept it
        if (!string.IsNullOrWhiteSpace(Input.InviteToken))
        {
            await _invitationService.AcceptInvitationAsync(Input.InviteToken!, user.MaNguoiDung);
        }

        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
        HttpContext.Session.SetString("UserName", user.HoTen);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToPage("/Index");
    }

    public class RegisterInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có từ {2} đến {1} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? AccountType { get; set; } = "Individual";
        public string? InviteToken { get; set; }
        [Display(Name = "Số điện thoại")]
        [RegularExpression("^\\+?\\d[\\d\\s]{7,14}\\d$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Phone { get; set; }
    }
}
