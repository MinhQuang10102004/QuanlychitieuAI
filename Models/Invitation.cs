using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class Invitation
    {
        [Key]
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Token { get; set; } = string.Empty;
        public int HouseholdId { get; set; }
        public int InviterId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Accepted { get; set; }
    }
}
