using Microsoft.AspNetCore.Mvc;
using QuanLyChiTieu.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Controllers
{
    public class AiController(AiService aiService, ChiTieuContext context, IMemoryCache cache, ILogger<AiController> logger, IWebHostEnvironment env) : ControllerBase
    {
        private readonly AiService _aiService = aiService;
        private readonly ChiTieuContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<AiController> _logger = logger;
        private readonly IWebHostEnvironment _env = env;

        [HttpPost("classify")]
        public async Task<IActionResult> ClassifyTransaction([FromBody] string description)
        {
            var result = await _aiService.ClassifyTransactionAsync(description);
            return Ok(result);
        }

        [HttpPost("ai/chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message)) return BadRequest("Missing message");

            // Auth check
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
            {
                return Unauthorized("Not signed in");
            }

            // Get AI response
            var reply = await _aiService.ChatAsync(req.Message);

            if (reply == null)
            {
                // Log and return 502 Bad Gateway to indicate upstream AI service failure
                _logger?.LogWarning("AI reply was null for user {UserId}", userId.Value);
                return StatusCode(502, "Có lỗi khi liên hệ trợ lý AI. Vui lòng thử lại sau.");
            }

            // Save to DB (only when we have a real reply)
            var chat = new QuanLyChiTieu.Models.ChatHistory
            {
                UserId = userId.Value,
                UserMessage = req.Message,
                AiReply = reply,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatHistories.Add(chat);
            await _context.SaveChangesAsync();

            return Ok(new { reply = reply });
        }

        [HttpGet("ai/history")]
        public async Task<IActionResult> History(int limit = 50)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
            {
                return Unauthorized("Not signed in");
            }

            var items = await _context.ChatHistories
                .Where(c => c.UserId == userId.Value)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .Select(c => new { c.UserMessage, c.AiReply, c.CreatedAt })
                .ToListAsync();

            items.Reverse();
            return Ok(items);
        }

        [HttpPost("ai/cleanup-errors")]
        public async Task<IActionResult> CleanupErrors()
        {
            // Development-only helper to remove previously stored error replies from ChatHistories
            if (!_env.IsDevelopment()) return Forbid();

            var q = _context.ChatHistories.Where(c => c.AiReply != null && (
                c.AiReply.Contains("Có lỗi khi liên hệ trợ lý AI") ||
                c.AiReply.Contains("AI không phản hồi") ||
                c.AiReply.Contains("Not signed in") ||
                c.AiReply.Contains("Lỗi:")
            ));

            var list = await q.ToListAsync();
            var count = list.Count;
            if (count > 0)
            {
                _context.ChatHistories.RemoveRange(list);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("CleanupErrors removed {Count} entries.", count);
            return Ok(new { removed = count });
        }

        public class ChatRequest { public string Message { get; set; } = string.Empty; }
    }
}
