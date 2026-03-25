using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Dtos;
using QuanLyChiTieu;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ThongKeController : ControllerBase
    {
        private readonly ChiTieuContext _context;

        public ThongKeController(ChiTieuContext context)
        {
            _context = context;
        }

        [HttpGet("monthly")]
        public async Task<ActionResult<IEnumerable<MonthlySummaryDto>>> GetMonthly([FromQuery] int? year, [FromQuery] int? userId)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var query = _context.GiaoDiches.AsNoTracking()
                .Where(g => g.NgayGiaoDich.Year == targetYear);

            if (userId.HasValue)
            {
                query = query.Where(g => g.MaNguoiDung == userId.Value);
            }

            var result = await query
                .GroupBy(g => g.NgayGiaoDich.Month)
                .Select(g => new MonthlySummaryDto(
                    g.Key,
                    g.Sum(x => x.SoTien),
                    g.Count()))
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-category")]
        public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetByCategory([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? userId)
        {
            var query = _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.DanhMuc)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(g => g.NgayGiaoDich >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date;
                query = query.Where(g => g.NgayGiaoDich <= toDate);
            }

            if (userId.HasValue)
            {
                query = query.Where(g => g.MaNguoiDung == userId.Value);
            }

            var result = await query
                .GroupBy(g => new { g.MaDanhMuc, TenDanhMuc = g.DanhMuc != null ? g.DanhMuc.TenDanhMuc : null })
                .Select(g => new CategorySummaryDto(
                    g.Key.MaDanhMuc,
                    g.Key.TenDanhMuc ?? string.Empty,
                    g.Sum(x => x.SoTien),
                    g.Count()))
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();

            return Ok(result);
        }
    }
}
