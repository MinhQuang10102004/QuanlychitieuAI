using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyChiTieu.Services;

namespace QuanLyChiTieu.Pages.AI;

public class IndexModel : PageModel
{
    private readonly AiService _aiService;
    // private readonly ChatBotService _chatBotService;
    // private readonly AiInsightService _aiInsightService;
    private const string InsightSessionKey = "AI:InsightSummary";

    public IndexModel(AiService aiService)
    // public IndexModel(AiService aiService, ChatBotService chatBotService, AiInsightService aiInsightService)
    {
        _aiService = aiService;
        // _chatBotService = chatBotService;
        // _aiInsightService = aiInsightService;
    }

    [BindProperty]
    public string ClassificationInput { get; set; } = string.Empty;

    [BindProperty]
    public string ConsultationInput { get; set; } = string.Empty;

    public string ClassificationResult { get; private set; } = string.Empty;

    public string ConsultationResult { get; private set; } = string.Empty;

    public string InsightResult { get; private set; } = string.Empty;

    public IActionResult OnGet()
    {
        var authResult = EnsureSignedInOrRedirect();
        if (authResult is not null)
        {
            return authResult;
        }

        ModelState.Clear();
        LoadInsightFromSession();
        return Page();
    }

    public async Task<IActionResult> OnPostClassifyAsync()
    {
        var authResult = EnsureSignedInOrRedirect();
        if (authResult is not null)
        {
            return authResult;
        }

        if (string.IsNullOrWhiteSpace(ClassificationInput))
        {
            ModelState.AddModelError(nameof(ClassificationInput), "Vui lòng nhập mô tả giao dịch.");
            return Page();
        }

        ClassificationResult = await _aiService.ClassifyTransactionAsync(ClassificationInput);
        ConsultationResult = string.Empty;
        LoadInsightFromSession();
        return Page();
    }

    public Task<IActionResult> OnPostConsultAsync()
    {
        var authResult = EnsureSignedInOrRedirect();
        if (authResult is not null)
        {
            return Task.FromResult(authResult);
        }

        if (string.IsNullOrWhiteSpace(ConsultationInput))
        {
            ModelState.AddModelError(nameof(ConsultationInput), "Vui lòng nhập câu hỏi cho trợ lý.");
            return Task.FromResult<IActionResult>(Page());
        }

        // ConsultationResult = await _chatBotService.GetFinancialAdviceAsync(ConsultationInput);
        ClassificationResult = string.Empty;
        LoadInsightFromSession();
        return Task.FromResult<IActionResult>(Page());
    }

    public Task<IActionResult> OnPostInsightAsync()
    {
        var authResult = EnsureSignedInOrRedirect();
        if (authResult is not null)
        {
            return Task.FromResult(authResult);
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return Task.FromResult<IActionResult>(RedirectToPage("/Account/Login"));
        }

        // InsightResult = await _aiInsightService.GenerateMonthlyInsightsAsync(userId.Value);
        if (string.IsNullOrWhiteSpace(InsightResult))
        {
            HttpContext.Session.Remove(InsightSessionKey);
        }
        else
        {
            HttpContext.Session.SetString(InsightSessionKey, InsightResult);
        }
        ClassificationResult = string.Empty;
        ConsultationResult = string.Empty;
        return Task.FromResult<IActionResult>(Page());
    }

    private IActionResult? EnsureSignedInOrRedirect()
    {
        return HttpContext.Session.GetInt32("UserId") == null
            ? RedirectToPage("/Account/Login")
            : null;
    }

    private void LoadInsightFromSession()
    {
        InsightResult = HttpContext.Session.GetString(InsightSessionKey) ?? string.Empty;
    }
}
