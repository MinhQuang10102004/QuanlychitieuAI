using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ChiTieuContext>();

            await context.Database.MigrateAsync();

            

            if (!await context.DanhMucs.AnyAsync())
            {
                var categories = new[]
                {
                    new DanhMuc { TenDanhMuc = "Ăn uống" },
                    new DanhMuc { TenDanhMuc = "Giải trí" },
                    new DanhMuc { TenDanhMuc = "Hóa đơn" },
                    new DanhMuc { TenDanhMuc = "Đi lại" },
                    new DanhMuc { TenDanhMuc = "Y tế" }
                };
                context.DanhMucs.AddRange(categories);
                await context.SaveChangesAsync();
            }

            if (!await context.GiaoDiches.AnyAsync())
            {
                var nguoiDungs = await context.NguoiDungs.ToListAsync();
                var danhMucs = await context.DanhMucs.ToListAsync();

                if (nguoiDungs.Count > 0 && danhMucs.Count > 1)
                {
                    var danhMucByName = danhMucs.ToDictionary(x => x.TenDanhMuc, StringComparer.OrdinalIgnoreCase);
                    var transactions = new List<GiaoDich>
                    {
                        new GiaoDich
                        {
                            MaNguoiDung = nguoiDungs[0].MaNguoiDung,
                            NguoiDung = nguoiDungs[0],
                            MaDanhMuc = danhMucByName["Ăn uống"].MaDanhMuc,
                            DanhMuc = danhMucByName["Ăn uống"],
                            SoTien = 150000,
                            NgayGiaoDich = DateTime.Today.AddDays(-1),
                            MoTa = "Bữa trưa với khách hàng"
                        },
                        new GiaoDich
                        {
                            MaNguoiDung = nguoiDungs[0].MaNguoiDung,
                            NguoiDung = nguoiDungs[0],
                            MaDanhMuc = danhMucByName["Hóa đơn"].MaDanhMuc,
                            DanhMuc = danhMucByName["Hóa đơn"],
                            SoTien = 950000,
                            NgayGiaoDich = DateTime.Today.AddDays(-5),
                            MoTa = "Thanh toán tiền điện"
                        },
                        new GiaoDich
                        {
                            MaNguoiDung = nguoiDungs[1].MaNguoiDung,
                            NguoiDung = nguoiDungs[1],
                            MaDanhMuc = danhMucByName["Giải trí"].MaDanhMuc,
                            DanhMuc = danhMucByName["Giải trí"],
                            SoTien = 320000,
                            NgayGiaoDich = DateTime.Today.AddDays(-8),
                            MoTa = "Xem phim cuối tuần"
                        },
                        new GiaoDich
                        {
                            MaNguoiDung = nguoiDungs[1].MaNguoiDung,
                            NguoiDung = nguoiDungs[1],
                            MaDanhMuc = danhMucByName["Đi lại"].MaDanhMuc,
                            DanhMuc = danhMucByName["Đi lại"],
                            SoTien = 120000,
                            NgayGiaoDich = DateTime.Today.AddDays(-2),
                            MoTa = "Đổ xăng"
                        },
                        new GiaoDich
                        {
                            MaNguoiDung = nguoiDungs[2].MaNguoiDung,
                            NguoiDung = nguoiDungs[2],
                            MaDanhMuc = danhMucByName["Y tế"].MaDanhMuc,
                            DanhMuc = danhMucByName["Y tế"],
                            SoTien = 450000,
                            NgayGiaoDich = DateTime.Today.AddDays(-15),
                            MoTa = "Khám sức khỏe"
                        }
                    };

                    context.GiaoDiches.AddRange(transactions);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
