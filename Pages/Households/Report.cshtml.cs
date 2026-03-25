using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Households
{
    public class ReportModel : PageModel
    {
        private readonly HouseholdFinanceService _financeService;

        public ReportModel(HouseholdFinanceService financeService)
        {
            _financeService = financeService;
        }

        public HouseholdReport? Report { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            // find user's household
            var hh = await _financeService.Context.HoGiaDinhs.FirstOrDefaultAsync(h => h.ThanhViens.Any(t => t.MaNguoiDung == userId.Value) || h.ChuHoId == userId.Value);
            if (hh == null) return Page();

            Report = await _financeService.GetHouseholdReportAsync(hh.MaHoGiaDinh);
            return Page();
        }
    }
}
