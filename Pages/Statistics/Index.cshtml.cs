using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyChiTieu.Pages.Statistics;

public class IndexModel : PageModel
{
    private readonly ChiTieuContext _context;

    public IndexModel(ChiTieuContext context)
    {
        _context = context;
    }

    public decimal TotalExpense { get; private set; }
    public int TotalTransactions { get; private set; }
    public string TopCategory { get; private set; } = "Chưa có dữ liệu";
    public IList<CategorySummary> CategoryBreakdown { get; private set; } = new List<CategorySummary>();
    public IList<MonthlySummary> MonthlySummaries { get; private set; } = new List<MonthlySummary>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        TotalTransactions = await _context.GiaoDiches.CountAsync();
        TotalExpense = await _context.GiaoDiches.SumAsync(g => (decimal?)g.SoTien) ?? 0m;

        CategoryBreakdown = await _context.GiaoDiches
            .AsNoTracking()
            .GroupBy(g => g.DanhMuc != null ? g.DanhMuc.TenDanhMuc : "Khác")
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(x => x.SoTien),
                TransactionCount = g.Count()
            })
            .OrderByDescending(g => g.TotalAmount)
            .ToListAsync();

        if (CategoryBreakdown.Count > 0)
        {
            TopCategory = CategoryBreakdown[0].CategoryName;
        }

        MonthlySummaries = await _context.GiaoDiches
            .AsNoTracking()
            .GroupBy(g => new { g.NgayGiaoDich.Year, g.NgayGiaoDich.Month })
            .Select(g => new MonthlySummary
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(x => x.SoTien)
            })
            .OrderByDescending(g => g.Year)
            .ThenByDescending(g => g.Month)
            .Take(6)
            .ToListAsync();

        MonthlySummaries = MonthlySummaries
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .Select(g =>
            {
                g.Label = new DateTime(g.Year, g.Month, 1).ToString("MM/yyyy");
                return g;
            })
            .ToList();

        return Page();
    }

    public class CategorySummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class MonthlySummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
