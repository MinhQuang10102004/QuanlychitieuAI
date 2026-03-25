using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Dtos;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DanhMucController : ControllerBase
    {
        private readonly ChiTieuContext _context;

        public DanhMucController(ChiTieuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhMucDto>>> GetAll()
        {
            var data = await _context.DanhMucs.AsNoTracking().ToListAsync();
            return Ok(data.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DanhMucDto>> GetById(int id)
        {
            var item = await _context.DanhMucs.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToDto(item));
        }

        [HttpPost]
        public async Task<ActionResult<DanhMucDto>> Create(CreateDanhMucRequest request)
        {
            var entity = new DanhMuc
            {
                TenDanhMuc = request.TenDanhMuc
            };

            _context.DanhMucs.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MaDanhMuc }, ToDto(entity));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateDanhMucRequest request)
        {
            var existing = await _context.DanhMucs.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TenDanhMuc = request.TenDanhMuc;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.DanhMucs.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            _context.DanhMucs.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static DanhMucDto ToDto(DanhMuc entity) => new(entity.MaDanhMuc, entity.TenDanhMuc);
    }
}
