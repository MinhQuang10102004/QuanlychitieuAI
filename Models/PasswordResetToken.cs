using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}
