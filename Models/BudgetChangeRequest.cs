using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class BudgetChangeRequest
    {
        [Key]
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public int RequesterId { get; set; }
        public decimal ProposedAmount { get; set; }
        public string ProposedStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public int? ReviewedById { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
