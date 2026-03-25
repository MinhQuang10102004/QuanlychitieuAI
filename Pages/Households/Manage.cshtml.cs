using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Services;
using QuanLyChiTieu;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Households
{
    public class ManageModel : PageModel
    {
        private readonly HouseholdService _householdService;
        private readonly InvitationService _invitationService;
        private readonly ChiTieuContext _context;

        public ManageModel(HouseholdService householdService, InvitationService invitationService, ChiTieuContext context)
        {
            _householdService = householdService;
            _invitationService = invitationService;
            _context = context;
        }

        public QuanLyChiTieu.Models.HoGiaDinh? CurrentHousehold { get; set; }
        public bool IsOwner { get; set; }
        public string? InviteToken { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            CurrentHousehold = await _context.HoGiaDinhs
                .Include(h => h.ChuHo)
                .Include(h => h.ThanhViens)
                .FirstOrDefaultAsync(h => h.ThanhViens.Any(v => v.MaNguoiDung == userId.Value));
            IsOwner = CurrentHousehold?.ChuHoId == userId.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync([FromForm] string Email)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            await _householdService.AddMemberByEmailAsync(userId.Value, Email);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostInviteAsync([FromForm] string? Email, [FromForm] string? Phone)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            var hh = await _householdService.GetUserHouseholdAsync(userId.Value);
            if (hh == null || hh.ChuHoId != userId.Value) return RedirectToPage();

            var token = await _invitationService.CreateInvitationAsync(userId.Value, hh.MaHoGiaDinh, Email, Phone);
            InviteToken = token;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync([FromForm] int memberId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            await _householdService.RemoveMemberAsync(userId.Value, memberId);
            return RedirectToPage();
        }
    }
}
