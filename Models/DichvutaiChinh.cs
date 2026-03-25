using System;

namespace QuanLyChiTieu.Models
{
    public class DichvutaiChinh
    {
        private readonly ChiTieuContext _db;
        public DichvutaiChinh(ChiTieuContext db)
        {
            _db = db;
        }

        public async Task UpdateBalance(int taiKhoanId, decimal amount, string moTa)
        {
            var tk = await _db.TaiKhoanTaiChinh.FindAsync(taiKhoanId);
            if (tk == null) return;

            // Cộng trừ số dư
            tk.SoDu += amount;

            // Lưu audit history
            _db.TaiKhoanLichSu.Add(new TaiKhoanLichSu
            {
                TaiKhoanId = taiKhoanId,
                SoTien = amount,
                MoTa = moTa
            });

            await _db.SaveChangesAsync();
        }
    }
}
