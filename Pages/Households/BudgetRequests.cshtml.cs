using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Services;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Households
{
    public class BudgetRequestsModel : PageModel
    {
        private readonly BudgetApprovalService _approvalService;
        private readonly ChiTieuContext _context;

        public BudgetRequestsModel(BudgetApprovalService approvalService, ChiTieuContext context)
        {
            _approvalService = approvalService;
            _context = context;
        }

        public List<BudgetChangeRequest>? Requests { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null) return RedirectToPage("/Account/Login");

            Requests = (await _approvalService.GetPendingRequestsForOwnerAsync(uid.Value)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null) return RedirectToPage("/Account/Login");

            await _approvalService.ApproveRequestAsync(id, uid.Value);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null) return RedirectToPage("/Account/Login");

            await _approvalService.RejectRequestAsync(id, uid.Value);
            return RedirectToPage();
        }
    }
}
