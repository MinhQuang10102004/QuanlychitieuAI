using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class HouseholdFinanceService
    {
        private readonly ChiTieuContext _context;
        private readonly ILogger<HouseholdFinanceService> _logger;

        public HouseholdFinanceService(ChiTieuContext context, ILogger<HouseholdFinanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // expose context for lightweight queries from pages (avoid extensive coupling)
        public ChiTieuContext Context => _context;

        public async Task<GiaoDich?> CreateSharedExpenseAsync(int ownerId, decimal amount, DateTime date, string description, int categoryId)
        {
            var hh = await _context.HoGiaDinhs.Include(h => h.ThanhViens).FirstOrDefaultAsync(h => h.ChuHoId == ownerId);
            if (hh == null) return null;

            var members = hh.ThanhViens.ToList();
            if (members.Count == 0) return null;

            var txn = new GiaoDich
            {
                MaNguoiDung = ownerId,
                MaDanhMuc = categoryId,
                SoTien = amount,
                NgayGiaoDich = date,
                MoTa = description,
                MaHoGiaDinh = hh.MaHoGiaDinh,
                IsShared = true
            };

            _context.GiaoDiches.Add(txn);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            // Split evenly among members
            var per = Math.Round(amount / members.Count, 2);
            var shares = new List<GiaoDichShare>();
            foreach (var m in members)
            {
                shares.Add(new GiaoDichShare
                {
                    GiaoDichId = txn.MaGiaoDich,
                    MaNguoiDung = m.MaNguoiDung,
                    Amount = per
                });
            }

            // adjust rounding difference to owner
            var totalAssigned = shares.Sum(s => s.Amount);
            if (totalAssigned != amount)
            {
                var diff = amount - totalAssigned;
                shares.First().Amount += diff;
            }

            _context.GiaoDichShares.AddRange(shares);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation("Created shared transaction {Txn} for household {Household}", txn.MaGiaoDich, hh.MaHoGiaDinh);
            return txn;
        }

        public async Task<HouseholdReport> GetHouseholdReportAsync(int householdId, DateTime? from = null, DateTime? to = null)
        {
            var hh = await _context.HoGiaDinhs.Include(h => h.ThanhViens).FirstOrDefaultAsync(h => h.MaHoGiaDinh == householdId);
            if (hh == null) return new HouseholdReport { Message = "Household not found" };

            var start = from ?? DateTime.UtcNow.AddMonths(-1);
            var end = to ?? DateTime.UtcNow;

            var txns = await _context.GiaoDiches
                .Where(g => g.MaHoGiaDinh == householdId && g.NgayGiaoDich >= start && g.NgayGiaoDich <= end)
                .ToListAsync();

            var totalExpense = txns.Where(t => t.SoTien < 0).Sum(t => t.SoTien);
            var totalIncome = txns.Where(t => t.SoTien > 0).Sum(t => t.SoTien);

            // per-member shares
            var memberIds = hh.ThanhViens.Select(m => m.MaNguoiDung).ToList();
            var shares = await _context.GiaoDichShares
                .Where(s => memberIds.Contains(s.MaNguoiDung) && txns.Select(t => t.MaGiaoDich).Contains(s.GiaoDichId))
                .ToListAsync();

            var perMember = memberIds.ToDictionary(id => id, id => shares.Where(s => s.MaNguoiDung == id).Sum(s => s.Amount));

            var report = new HouseholdReport
            {
                HouseholdId = householdId,
                HouseholdName = hh.TenHoGiaDinh,
                TotalIncome = totalIncome,
                TotalExpense = Math.Abs(totalExpense),
                PerMemberShare = perMember,
                Message = "OK"
            };

            report.Savings = report.TotalIncome - report.TotalExpense;
            return report;
        }
    }

    public class HouseholdReport
    {
        public int HouseholdId { get; set; }
        public string HouseholdName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Savings { get; set; }
        public Dictionary<int, decimal> PerMemberShare { get; set; } = new Dictionary<int, decimal>();
        public string Message { get; set; } = string.Empty;
    }
}
