using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Budgets
{
    public class EditModel : PageModel
    {
        private readonly ChiTieuContext _context;
        private readonly BudgetApprovalService _approvalService;

        public EditModel(ChiTieuContext context, BudgetApprovalService approvalService)
        {
            _context = context;
            _approvalService = approvalService;
        }

        [BindProperty]
        public NganSach? Budget { get; set; }

        [BindProperty]
        [Required]
        [Range(0, 100000000000)]
        public decimal Amount { get; set; }

        [BindProperty]
        public string Status { get; set; } = "Active";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");
            Budget = await _context.Budgets.FindAsync(id);
            if (Budget == null) return NotFound();

            var owner = await _context.NguoiDungs.FindAsync(Budget.MaNguoiDung);
            var currentUser = await _context.NguoiDungs.FindAsync(userId.Value);
            var isOwner = owner != null && owner.MaNguoiDung == userId.Value;
            var isSameHousehold = owner?.MaHoGiaDinh != null && owner.MaHoGiaDinh == currentUser?.MaHoGiaDinh;

            if (!isOwner && !isSameHousehold) return Forbid();

            Amount = Budget.SoTien;
            Status = Budget.TinhTrang;

            ViewData["IsOwner"] = isOwner;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            Budget = await _context.Budgets.FindAsync(id);
            if (Budget == null) return NotFound();

            var owner = await _context.NguoiDungs.FindAsync(Budget.MaNguoiDung);
            var currentUser = await _context.NguoiDungs.FindAsync(userId.Value);
            var isOwner = owner != null && owner.MaNguoiDung == userId.Value;
            var isSameHousehold = owner?.MaHoGiaDinh != null && owner.MaHoGiaDinh == currentUser?.MaHoGiaDinh;

            if (!isOwner && !isSameHousehold) return Forbid();

            if (!ModelState.IsValid) return Page();

            if (isOwner)
            {
                Budget.SoTien = Amount;
                Budget.TinhTrang = Status;
                _context.Budgets.Update(Budget);
                await _context.SaveChangesAsync();
                return RedirectToPage("Details", new { id = Budget.BudgetId });
            }

            // Non-owner (household member) — create approval request
            await _approvalService.CreateRequestAsync(Budget.BudgetId, userId.Value, Amount, Status);
            TempData["Message"] = "Thay đổi đã được gửi tới chủ hộ để duyệt.";
            return RedirectToPage("Details", new { id = Budget.BudgetId });
        }
    }
}
