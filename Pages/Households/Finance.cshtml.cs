using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Households
{
    public class FinanceModel : PageModel
    {
        private readonly HouseholdFinanceService _financeService;

        public FinanceModel(HouseholdFinanceService financeService)
        {
            _financeService = financeService;
        }

        [BindProperty]
        [Required]
        public decimal Amount { get; set; }

        [BindProperty]
        public int CategoryId { get; set; }

        [BindProperty]
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [BindProperty]
        public string Description { get; set; } = string.Empty;

        public int? CreatedTxnId { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            var txn = await _financeService.CreateSharedExpenseAsync(userId.Value, Amount, Date, Description, CategoryId);
            if (txn != null) CreatedTxnId = txn.MaGiaoDich;
            return Page();
        }
    }
}
