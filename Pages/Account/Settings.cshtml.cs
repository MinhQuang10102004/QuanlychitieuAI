using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using QuanLyChiTieu.Services;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Account;

public class SettingsModel : PageModel
{
    private readonly InvitationService _invitationService;
    private readonly ChiTieuContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SettingsModel> _logger;

    public SettingsModel(InvitationService invitationService, ChiTieuContext context, IWebHostEnvironment env, ILogger<SettingsModel> logger)
    {
        _invitationService = invitationService;
        _context = context;
        _env = env;
        _logger = logger;
    }

    [BindProperty]
    public string? InviteToken { get; set; }

    public string? Message { get; set; }

    public string? UserName { get; set; }
    public HoGiaDinh? Household { get; set; }
    public List<Invitation>? IncomingInvitations { get; set; }
    public List<Invitation>? SentInvitations { get; set; }
    public Dictionary<int, string>? InviterNames { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToPage("/Account/Login");
        UserName = HttpContext.Session.GetString("UserName");
        await LoadListsAsync(uid.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptInvite()
    {
        var uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToPage("/Account/Login");

        if (string.IsNullOrWhiteSpace(InviteToken))
        {
            Message = "Vui lòng nhập mã mời.";
            UserName = HttpContext.Session.GetString("UserName");
            return Page();
        }

        var ok = await _invitationService.AcceptInvitationAsync(InviteToken.Trim(), uid.Value);
        Message = ok ? "Bạn đã gia nhập hộ gia đình thành công." : "Mã mời không hợp lệ hoặc đã hết hạn.";
        UserName = HttpContext.Session.GetString("UserName");
        await LoadListsAsync(uid.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostCancelInviteAsync(string token)
    {
        var uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToPage("/Account/Login");

        var ok = await _invitationService.CancelInvitationAsync(token, uid.Value);
        Message = ok ? "Lời mời đã được hủy." : "Không thể hủy lời mời (bạn không có quyền hoặc lời mời đã thay đổi).";
        await LoadListsAsync(uid.Value);
        UserName = HttpContext.Session.GetString("UserName");
        return Page();
    }

    private async Task LoadListsAsync(int uid)
    {
        IncomingInvitations = (await _invitationService.GetPendingInvitationsForUserAsync(uid)).ToList();
        SentInvitations = (await _invitationService.GetSentInvitationsByUserAsync(uid)).ToList();

        // Load inviter names for display - should be from IncomingInvitations
        var allInviterIds = IncomingInvitations.Select(i => i.InviterId).Distinct().ToList();
        if (allInviterIds.Any())
        {
            var inviters = await _context.NguoiDungs
                .Where(u => allInviterIds.Contains(u.MaNguoiDung))
                .Select(u => new { u.MaNguoiDung, u.HoTen })
                .ToListAsync();
            InviterNames = inviters.ToDictionary(x => x.MaNguoiDung, x => x.HoTen);
        }
        else
        {
            InviterNames = new Dictionary<int, string>();
        }

        var user = await _context.NguoiDungs.FindAsync(uid);
        if (user?.MaHoGiaDinh != null)
        {
            Household = await _context.HoGiaDinhs
                .Include(h => h.ChuHo)
                .FirstOrDefaultAsync(h => h.MaHoGiaDinh == user.MaHoGiaDinh);
        }
    }

    public async Task<IActionResult> OnPostCleanupAiErrorsAsync()
    {
        var uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToPage("/Account/Login");

        if (!_env.IsDevelopment())
        {
            Message = "Chức năng này chỉ khả dụng trong môi trường phát triển.";
            UserName = HttpContext.Session.GetString("UserName");
            await LoadListsAsync(uid.Value);
            return Page();
        }

        var q = _context.ChatHistories.Where(c => c.AiReply != null && (
            c.AiReply.Contains("Có lỗi khi liên hệ trợ lý AI") ||
            c.AiReply.Contains("AI không phản hồi") ||
            c.AiReply.Contains("Not signed in") ||
            c.AiReply.Contains("Lỗi:")
        ));

        var list = await q.ToListAsync();
        var count = list.Count;
        if (count > 0)
        {
            _context.ChatHistories.RemoveRange(list);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Settings cleanup removed {Count} entries by user {UserId}.", count, uid.Value);
        Message = $"Đã xóa {count} bản ghi lịch sử lỗi AI.";
        UserName = HttpContext.Session.GetString("UserName");
        await LoadListsAsync(uid.Value);
        return Page();
    }
}
