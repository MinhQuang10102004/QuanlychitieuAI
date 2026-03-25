using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.Budgets
{
    public class CreateBySalaryModel : PageModel
    {
        private readonly SalaryBudgetService _salaryBudgetService;

        public CreateBySalaryModel(SalaryBudgetService salaryBudgetService)
        {
            _salaryBudgetService = salaryBudgetService;
        }

        [BindProperty]
        [Required]
        [Range(0, 1000000000)]
        public decimal Salary { get; set; }

        [BindProperty]
        [Required]
        public DateTime Month { get; set; }

        public List<NganSach>? CreatedBudgets { get; private set; }

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

            // Normalize Month to first day
            Month = new DateTime(Month.Year, Month.Month, 1);

            CreatedBudgets = await _salaryBudgetService.CreateMonthlyBudgetsAsync(userId.Value, Salary, Month);
            return Page();
        }
    }
}
