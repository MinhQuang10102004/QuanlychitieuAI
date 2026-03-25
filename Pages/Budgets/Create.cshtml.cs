using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Budgets
{
    public class CreateModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public CreateModel(ChiTieuContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [Range(0, 100000000000)]
        public decimal Amount { get; set; }

        [BindProperty]
        [Required]
        public DateTime Start { get; set; } = DateTime.UtcNow.Date;

        [BindProperty]
        [Required]
        public DateTime End { get; set; } = DateTime.UtcNow.Date.AddMonths(1).AddDays(-1);

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var b = new NganSach
            {
                MaNguoiDung = userId.Value,
                TenBudget = Name,
                SoTien = Amount,
                ThoiGianBatDau = Start,
                ThoiGianKetThuc = End,
                TinhTrang = "Active",
                NgayTao = DateTime.UtcNow
            };

            _context.Budgets.Add(b);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tạo ngân sách thành công.";
            return RedirectToPage("Details", new { id = b.BudgetId });
        }
    }
}