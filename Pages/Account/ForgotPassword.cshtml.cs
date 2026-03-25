using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly ChiTieuContext _context;
    private readonly INotificationService _notifier;

    public ForgotPasswordModel(ChiTieuContext context, INotificationService notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string Message { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostSend()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Find user by email or phone
        var contact = Input.Contact.Trim();
        var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == contact || u.Phone == contact);
        if (user == null)
        {
            // Do not reveal whether user exists
            Message = "Nếu tài khoản tồn tại, chúng tôi sẽ gửi hướng dẫn khôi phục.";
            ViewData["SuccessMessage"] = Message;
            return Page();
        }

        // generate token and persist
        var token = Guid.NewGuid().ToString("N");
        var expires = DateTime.UtcNow.AddHours(1);

        var pr = new QuanLyChiTieu.Models.PasswordResetToken
        {
            UserId = user.MaNguoiDung,
            Token = token,
            ExpiresAt = expires
        };
        _context.PasswordResetTokens.Add(pr);
        await _context.SaveChangesAsync();

        // prepare link - in production build absolute URL from config
        var resetUrl = Url.Page("/Account/ResetPassword", null, new { token }, Request.Scheme);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var subj = "Yêu cầu đặt lại mật khẩu";
            var body = $"Bạn (hoặc ai đó) đã yêu cầu đặt lại mật khẩu. Mở liên kết sau để đặt mật khẩu mới:\n{resetUrl}\nLiên kết có hiệu lực đến {expires:u} (UTC).";
            await _notifier.SendEmailAsync(user.Email, subj, body);
        }

        if (!string.IsNullOrWhiteSpace(user.Phone))
        {
            var msg = $"Mã đặt lại mật khẩu: {token}. Hoặc mở {resetUrl}";
            await _notifier.SendSmsAsync(user.Phone, msg);
        }

        Message = "Nếu tài khoản tồn tại, chúng tôi đã gửi hướng dẫn khôi phục tới email hoặc số điện thoại liên kết.";
        ViewData["SuccessMessage"] = Message;
        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
        public string Contact { get; set; } = string.Empty;
    }
}
