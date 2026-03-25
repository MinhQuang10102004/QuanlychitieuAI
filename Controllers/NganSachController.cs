using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System;

namespace QuanLyChiTieu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NganSachController : ControllerBase
    {
        private readonly ChiTieuContext _db;
        private readonly DichvuNgansach _budgetService;

        public NganSachController(ChiTieuContext db, DichvuNgansach budgetService)
        {
            _db = db;
            _budgetService = budgetService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NganSach model)
        {
            _db.Budgets.Add(model);
            await _db.SaveChangesAsync();
            return Ok(model);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var list = await _db.Budgets.Where(b => b.MaNguoiDung == userId).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}/usage")]
        public async Task<IActionResult> GetUsage(int id)
        {
            var percent = await _budgetService.GetUsagePercent(id);
            var used = await _budgetService.GetTotalUsed(id);
            return Ok(new { percent, used });
        }
    }
}
