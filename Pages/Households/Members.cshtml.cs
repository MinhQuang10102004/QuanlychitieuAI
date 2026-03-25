using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Households
{
    public class MembersModel : PageModel
    {
        private readonly ChiTieuContext _context;
        private readonly HouseholdService _householdService;

        public MembersModel(ChiTieuContext context, HouseholdService householdService)
        {
            _context = context;
            _householdService = householdService;
        }

        public HoGiaDinh? Household { get; set; }
        public bool IsOwner { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            Household = await _context.HoGiaDinhs
                .Include(h => h.ThanhViens)
                .FirstOrDefaultAsync(h => h.ThanhViens.Any(v => v.MaNguoiDung == userId.Value));

            if (Household == null)
                return RedirectToPage("/Households/Index");

            IsOwner = Household.ChuHoId == userId.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveMemberAsync(int memberId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            await _householdService.RemoveMemberAsync(userId.Value, memberId);
            return RedirectToPage();
        }
    }
}
