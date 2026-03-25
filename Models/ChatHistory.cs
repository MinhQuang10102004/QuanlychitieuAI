using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class ChatHistory
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserMessage { get; set; } = string.Empty;

        public string AiReply { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
