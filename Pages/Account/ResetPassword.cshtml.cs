using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly ChiTieuContext _context;
    private readonly IPasswordHasher<QuanLyChiTieu.Models.NguoiDung> _passwordHasher;

    public ResetPasswordModel(ChiTieuContext context, IPasswordHasher<QuanLyChiTieu.Models.NguoiDung> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string Message { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostChange()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
            return Page();
        }

        // If token provided -> validate and reset
        if (!string.IsNullOrWhiteSpace(Input.Token))
        {
            var token = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == Input.Token);
            if (token == null || token.ExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Mã xác thực không hợp lệ hoặc đã hết hạn.");
                return Page();
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == token.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Người dùng không tồn tại.");
                return Page();
            }

            user.MatKhau = _passwordHasher.HashPassword(user, Input.NewPassword);
            _context.PasswordResetTokens.Remove(token);
            await _context.SaveChangesAsync();

            Message = "Mật khẩu đã được đặt lại thành công.";
            ViewData["SuccessMessage"] = Message;
            return Page();
        }

        // No token: if user is signed in, allow password change
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            ModelState.AddModelError(string.Empty, "Bạn phải có mã xác thực hoặc đăng nhập để đổi mật khẩu.");
            return Page();
        }

        var currentUser = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == userId.Value);
        if (currentUser == null)
        {
            ModelState.AddModelError(string.Empty, "Người dùng không tồn tại.");
            return Page();
        }

        currentUser.MatKhau = _passwordHasher.HashPassword(currentUser, Input.NewPassword);
        await _context.SaveChangesAsync();

        Message = "Mật khẩu đã được cập nhật.";
        ViewData["SuccessMessage"] = Message;
        return Page();
    }

    public class InputModel
    {
        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
