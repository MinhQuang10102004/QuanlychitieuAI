using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class SalaryBudgetService
    {
        private readonly ChiTieuContext _context;
        private readonly ILogger<SalaryBudgetService> _logger;
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

        public SalaryBudgetService(ChiTieuContext context, ILogger<SalaryBudgetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<(string Category, decimal Amount, decimal Percent)> SuggestAllocations(decimal monthlySalary)
        {
            // Default allocation percentages (sum = 100)
            var allocations = new Dictionary<string, decimal>
            {
                ["Ăn uống"] = 30m,
                ["Hóa đơn"] = 25m,
                ["Đi lại"] = 10m,
                ["Giải trí"] = 10m,
                ["Tiết kiệm"] = 15m,
                ["Khác"] = 10m
            };

            return allocations.Select(kv => (kv.Key, Math.Round(monthlySalary * kv.Value / 100m, 2), kv.Value));
        }

        public async Task<List<NganSach>> CreateMonthlyBudgetsAsync(int userId, decimal monthlySalary, DateTime month)
        {
            var allocations = SuggestAllocations(monthlySalary).ToList();
            var start = new DateTime(month.Year, month.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var budgets = new List<NganSach>();
            foreach (var a in allocations)
            {
                var nb = new NganSach
                {
                    MaNguoiDung = userId,
                    TenBudget = $"Ngân sách: {a.Category} ({start:MM/yyyy})",
                    SoTien = a.Amount,
                    ThoiGianBatDau = start,
                    ThoiGianKetThuc = end,
                    TinhTrang = "Active",
                    NgayTao = DateTime.UtcNow
                };

                _context.Budgets.Add(nb);
                budgets.Add(nb);
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Tạo {Count} ngân sách cho user {UserId} cho tháng {Month}", budgets.Count, userId, start.ToString("MM/yyyy"));
            return budgets;
        }
    }
}
