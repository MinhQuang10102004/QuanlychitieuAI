using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Collections.Generic;

namespace QuanLyChiTieu
{
    public class ChiTieuContext : DbContext
    {
        public ChiTieuContext(DbContextOptions<ChiTieuContext> options) : base(options) { }
        public DbSet<GiaoDich> GiaoDiches { get; set; }
        public DbSet<GiaoDichShare> GiaoDichShares { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<Models.PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<HoGiaDinh> HoGiaDinhs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<NganSach> Budgets { get; set; }
        public DbSet<BudgetChangeRequest> BudgetChangeRequests { get; set; }
        public DbSet<LichSuNganSach> BudgetHistories { get; set; }
        public DbSet<TaiKhoanTaiChinh> TaiKhoanTaiChinh { get; set; }
        public DbSet<TaiKhoanLichSu> TaiKhoanLichSu { get; set; }
        public DbSet<Models.ChatHistory> ChatHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NganSach>()
                .HasMany(b => b.Histories)
                .WithOne(h => h.Budget!)
                .HasForeignKey(h => h.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Household has a single owner (ChuHo). Map ChuHo relationship.
            modelBuilder.Entity<HoGiaDinh>()
                .HasOne(h => h.ChuHo)
                .WithMany()
                .HasForeignKey(h => h.ChuHoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Map relationship: NguoiDung.MaHoGiaDinh -> HoGiaDinh.ThanhViens
            modelBuilder.Entity<HoGiaDinh>()
                .HasMany(h => h.ThanhViens)
                .WithOne(u => u.HoGiaDinh)
                .HasForeignKey(u => u.MaHoGiaDinh)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<GiaoDich>()
                .HasOne<HoGiaDinh>()
                .WithMany()
                .HasForeignKey(g => g.MaHoGiaDinh)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BudgetChangeRequest>()
                .HasOne<NganSach>()
                .WithMany()
                .HasForeignKey(b => b.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
