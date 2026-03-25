using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Pages.Transactions
{
    public class CreateModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public CreateModel(ChiTieuContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList NguoiDungOptions { get; private set; } = default!;
        public SelectList DanhMucOptions { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            await LoadOptionsAsync();
            Input.NgayGiaoDich = DateTime.Today;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            await LoadOptionsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(Input.MaNguoiDung);
            var danhMuc = await _context.DanhMucs.FindAsync(Input.MaDanhMuc);

            if (nguoiDung == null || danhMuc == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng hoặc danh mục đã chọn");
                return Page();
            }

            var entity = new GiaoDich
            {
                MaNguoiDung = Input.MaNguoiDung,
                NguoiDung = nguoiDung,
                MaDanhMuc = Input.MaDanhMuc,
                DanhMuc = danhMuc,
                SoTien = Input.SoTien,
                NgayGiaoDich = Input.NgayGiaoDich,
                MoTa = Input.MoTa
            };

            _context.GiaoDiches.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private IActionResult? RequireLogin()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToPage("/Account/Login");
            }

            return null;
        }

        private async Task LoadOptionsAsync()
        {
            var nguoiDung = await _context.NguoiDungs
                .AsNoTracking()
                .OrderBy(x => x.HoTen)
                .ToListAsync();
            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .OrderBy(x => x.TenDanhMuc)
                .ToListAsync();

            NguoiDungOptions = new SelectList(nguoiDung, nameof(NguoiDung.MaNguoiDung), nameof(NguoiDung.HoTen));
            DanhMucOptions = new SelectList(danhMuc, nameof(DanhMuc.MaDanhMuc), nameof(DanhMuc.TenDanhMuc));
        }

        public class InputModel
        {
            [Display(Name = "Người dùng")]
            [Required(ErrorMessage = "Vui lòng chọn người dùng")]
            public int MaNguoiDung { get; set; }

            [Display(Name = "Danh mục")]
            [Required(ErrorMessage = "Vui lòng chọn danh mục")]
            public int MaDanhMuc { get; set; }

            [Display(Name = "Số tiền")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
            public decimal SoTien { get; set; }

            [Display(Name = "Ngày giao dịch")]
            [DataType(DataType.Date)]
            public DateTime NgayGiaoDich { get; set; }

            [Display(Name = "Mô tả")]
            public string? MoTa { get; set; }
        }
    }
}
