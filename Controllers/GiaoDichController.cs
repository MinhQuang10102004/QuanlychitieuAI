using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Dtos;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GiaoDichController : ControllerBase
    {
        private readonly ChiTieuContext _context;

        public GiaoDichController(ChiTieuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiaoDichDto>>> GetAll()
        {
            var data = await _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.DanhMuc)
                .Include(g => g.NguoiDung)
                .ToListAsync();

            return Ok(data.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GiaoDichDto>> GetById(int id)
        {
            var giaoDich = await _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.DanhMuc)
                .Include(g => g.NguoiDung)
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id);

            if (giaoDich == null)
            {
                return NotFound();
            }

            return Ok(ToDto(giaoDich));
        }

        [HttpPost]
        public async Task<ActionResult<GiaoDichDto>> Create(CreateGiaoDichRequest request)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(request.MaNguoiDung);
            if (nguoiDung == null)
            {
                return BadRequest($"Không tìm thấy người dùng với mã {request.MaNguoiDung}");
            }

            var danhMuc = await _context.DanhMucs.FindAsync(request.MaDanhMuc);
            if (danhMuc == null)
            {
                return BadRequest($"Không tìm thấy danh mục với mã {request.MaDanhMuc}");
            }

            var entity = new GiaoDich
            {
                MaNguoiDung = request.MaNguoiDung,
                MaDanhMuc = request.MaDanhMuc,
                SoTien = request.SoTien,
                NgayGiaoDich = request.NgayGiaoDich,
                MoTa = request.MoTa,
                NguoiDung = nguoiDung,
                DanhMuc = danhMuc
            };

            _context.GiaoDiches.Add(entity);
            await _context.SaveChangesAsync();

            var created = await _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.DanhMuc)
                .Include(g => g.NguoiDung)
                .FirstAsync(g => g.MaGiaoDich == entity.MaGiaoDich);

            return CreatedAtAction(nameof(GetById), new { id = created.MaGiaoDich }, ToDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateGiaoDichRequest request)
        {
            var entity = await _context.GiaoDiches.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (entity.MaNguoiDung != request.MaNguoiDung)
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(request.MaNguoiDung);
                if (nguoiDung == null)
                {
                    return BadRequest($"Không tìm thấy người dùng với mã {request.MaNguoiDung}");
                }

                entity.MaNguoiDung = request.MaNguoiDung;
                entity.NguoiDung = nguoiDung;
            }

            if (entity.MaDanhMuc != request.MaDanhMuc)
            {
                var danhMuc = await _context.DanhMucs.FindAsync(request.MaDanhMuc);
                if (danhMuc == null)
                {
                    return BadRequest($"Không tìm thấy danh mục với mã {request.MaDanhMuc}");
                }

                entity.MaDanhMuc = request.MaDanhMuc;
                entity.DanhMuc = danhMuc;
            }

            entity.SoTien = request.SoTien;
            entity.NgayGiaoDich = request.NgayGiaoDich;
            entity.MoTa = request.MoTa;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.GiaoDiches.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.GiaoDiches.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static GiaoDichDto ToDto(GiaoDich entity)
        {
            var hoTen = entity.NguoiDung?.HoTen ?? string.Empty;
            var tenDanhMuc = entity.DanhMuc?.TenDanhMuc ?? string.Empty;
            return new GiaoDichDto(
                entity.MaGiaoDich,
                entity.MaNguoiDung,
                hoTen,
                entity.MaDanhMuc,
                tenDanhMuc,
                entity.SoTien,
                entity.NgayGiaoDich,
                entity.MoTa);
        }

    }
}
