using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Services
{
    public class HouseholdService
    {
        private readonly ChiTieuContext _context;
        private readonly ILogger<HouseholdService> _logger;

        public HouseholdService(ChiTieuContext context, ILogger<HouseholdService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HoGiaDinh?> CreateHouseholdAsync(int ownerId, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var owner = await _context.NguoiDungs.FindAsync(ownerId);
            if (owner == null) return null;

            var hh = new HoGiaDinh
            {
                TenHoGiaDinh = name,
                ChuHoId = ownerId
            };

            _context.HoGiaDinhs.Add(hh);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            owner.MaHoGiaDinh = hh.MaHoGiaDinh;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return hh;
        }

        public async Task<bool> AddMemberByEmailAsync(int ownerId, string memberEmail)
        {
            if (string.IsNullOrWhiteSpace(memberEmail)) return false;

            var hh = await _context.HoGiaDinhs.FirstOrDefaultAsync(h => h.ChuHoId == ownerId);
            if (hh == null) return false;

            var member = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == memberEmail);
            if (member == null) return false;

            member.MaHoGiaDinh = hh.MaHoGiaDinh;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Added member {Email} to household {Household}", memberEmail, hh.MaHoGiaDinh);
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int ownerId, int memberId)
        {
            var hh = await _context.HoGiaDinhs.FirstOrDefaultAsync(h => h.ChuHoId == ownerId);
            if (hh == null) return false;

            var member = await _context.NguoiDungs.FindAsync(memberId);
            if (member == null || member.MaHoGiaDinh != hh.MaHoGiaDinh) return false;

            member.MaHoGiaDinh = null;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Removed member {Member} from household {Household}", memberId, hh.MaHoGiaDinh);
            return true;
        }

        public async Task<HoGiaDinh?> GetHouseholdByIdAsync(int householdId)
        {
            return await _context.HoGiaDinhs
                .Include(h => h.ChuHo)
                .Include(h => h.ThanhViens)
                .FirstOrDefaultAsync(h => h.MaHoGiaDinh == householdId)
                .ConfigureAwait(false);
        }

        public async Task<HoGiaDinh?> GetUserHouseholdAsync(int userId)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user?.MaHoGiaDinh == null) return null;
            return await GetHouseholdByIdAsync(user.MaHoGiaDinh.Value).ConfigureAwait(false);
        }
    }
}
