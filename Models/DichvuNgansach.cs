using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Models
{
    public class DichvuNgansach
    {
        private readonly ChiTieuContext _db;
        public DichvuNgansach(ChiTieuContext db) => _db = db;

        // Tìm budget active cho user đang chứa ngày giao dịch
        public async Task<NganSach?> GetActiveBudgetForUser(int userId, DateTime date)
        {
            return await _db.Budgets
                .Where(b => b.MaNguoiDung == userId
                            && b.ThoiGianBatDau <= date
                            && b.ThoiGianKetThuc >= date
                            && b.TinhTrang == "Active")
                .FirstOrDefaultAsync();
        }

        // Khi có giao dịch: record vào BudgetHistory (soTien âm nếu là chi)
        public async Task RecordTransactionToBudget(int userId, DateTime ngay, decimal amount, string? ghiChu = null)
        {
            var budget = await GetActiveBudgetForUser(userId, ngay);
            if (budget == null) return;

            var history = new LichSuNganSach
            {
                BudgetId = budget.BudgetId,
                Ngay = ngay,
                SoTien = amount, // amount negative for chi, positive for thu
                Loai = "GiaoDich",
                GhiChu = ghiChu
            };
            _db.BudgetHistories.Add(history);
            await _db.SaveChangesAsync();
        }

        // Lấy tổng đã dùng trong budget
        public async Task<decimal> GetTotalUsed(int budgetId)
        {
            return await _db.BudgetHistories
                .Where(h => h.BudgetId == budgetId)
                .SumAsync(h => (decimal?)h.SoTien) ?? 0m;
        }

        // Kiểm tra ngưỡng: trả về % đã dùng
        public async Task<decimal> GetUsagePercent(int budgetId)
        {
            var budget = await _db.Budgets.FindAsync(budgetId);
            if (budget == null) return 0m;
            var used = await GetTotalUsed(budgetId);
            // Lưu ý: budget.SoTien là tổng ngân sách đặt ra, used có thể âm (nếu lưu -ve for chi)
            var usedAbs = Math.Abs(used);
            return budget.SoTien == 0 ? 0 : Math.Min(100m, (usedAbs / budget.SoTien) * 100m);
        }
    }
}
