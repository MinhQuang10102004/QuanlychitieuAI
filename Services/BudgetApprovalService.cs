using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class BudgetApprovalService
    {
        private readonly ChiTieuContext _context;
        private readonly ILogger<BudgetApprovalService> _logger;

        public BudgetApprovalService(ChiTieuContext context, ILogger<BudgetApprovalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BudgetChangeRequest> CreateRequestAsync(int budgetId, int requesterId, decimal amount, string status, string? note = null)
        {
            var req = new BudgetChangeRequest
            {
                BudgetId = budgetId,
                RequesterId = requesterId,
                ProposedAmount = amount,
                ProposedStatus = status,
                Note = note
            };
            _context.BudgetChangeRequests.Add(req);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created budget change request {Id} for budget {Budget} by {User}", req.Id, budgetId, requesterId);
            return req;
        }

        public async Task<IEnumerable<BudgetChangeRequest>> GetPendingRequestsForOwnerAsync(int ownerUserId)
        {
            // find households owned by user, then budgets owned by household members? Simpler: find budgets where owner is in an owned household
            var ownedHouseholdIds = await _context.HoGiaDinhs
                .Where(h => h.ChuHoId == ownerUserId)
                .Select(h => h.MaHoGiaDinh)
                .ToListAsync();

            var memberIds = await _context.NguoiDungs
                .Where(u => u.MaHoGiaDinh != null && ownedHouseholdIds.Contains(u.MaHoGiaDinh.Value))
                .Select(u => u.MaNguoiDung)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Where(b => memberIds.Contains(b.MaNguoiDung))
                .Select(b => b.BudgetId)
                .ToListAsync();

            return await _context.BudgetChangeRequests
                .Where(r => r.Status == "Pending" && budgets.Contains(r.BudgetId))
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveRequestAsync(int requestId, int reviewerId)
        {
            var req = await _context.BudgetChangeRequests.FindAsync(requestId);
            if (req == null || req.Status != "Pending") return false;

            var budget = await _context.Budgets.FindAsync(req.BudgetId);
            if (budget == null) return false;

            budget.SoTien = req.ProposedAmount;
            budget.TinhTrang = req.ProposedStatus;
            req.Status = "Approved";
            req.ReviewedById = reviewerId;
            req.ReviewedAt = System.DateTime.UtcNow;

            _context.Budgets.Update(budget);
            _context.BudgetChangeRequests.Update(req);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Budget change request {Id} approved by {User}", requestId, reviewerId);
            return true;
        }

        public async Task<bool> RejectRequestAsync(int requestId, int reviewerId, string? reason = null)
        {
            var req = await _context.BudgetChangeRequests.FindAsync(requestId);
            if (req == null || req.Status != "Pending") return false;
            req.Status = "Rejected";
            req.ReviewedById = reviewerId;
            req.ReviewedAt = System.DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason)) req.Note = (req.Note ?? "") + "\n[Rejected Reason]: " + reason;
            _context.BudgetChangeRequests.Update(req);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Budget change request {Id} rejected by {User}", requestId, reviewerId);
            return true;
        }
    }
}
