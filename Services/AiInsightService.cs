using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanLyChiTieu;

namespace QuanLyChiTieu.Services;

public class AiInsightService
{
    private readonly ChiTieuContext _context;
    private readonly OpenAIClient _client;
    private readonly ILogger<AiInsightService> _logger;
    private readonly string _deploymentName;
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    public AiInsightService(ChiTieuContext context, OpenAIClient client, IConfiguration configuration, ILogger<AiInsightService> logger)
    {
        _context = context;
        _client = client;
        _logger = logger;
        _deploymentName = configuration["OpenAI:InsightDeployment"]
            ?? configuration["OpenAI:DeploymentName"]
            ?? throw new InvalidOperationException("Chưa cấu hình tên deployment cho OpenAI. Vui lòng đặt OpenAI:DeploymentName hoặc OpenAI:InsightDeployment.");
    }

    public async Task<string> GenerateMonthlyInsightsAsync(int userId, DateTime? referenceDate = null, CancellationToken cancellationToken = default)
    {
        var today = (referenceDate ?? DateTime.UtcNow).Date;
        var startDate = new DateTime(today.Year, today.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _context.GiaoDiches
            .AsNoTracking()
            .Include(g => g.DanhMuc)
            .Where(g => g.MaNguoiDung == userId && g.NgayGiaoDich.Date >= startDate && g.NgayGiaoDich.Date <= endDate)
            .OrderBy(g => g.NgayGiaoDich)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (transactions.Count == 0)
        {
            return "Chưa ghi nhận giao dịch nào trong tháng này nên không thể tạo báo cáo chi tiêu.";
        }

        var totalSpending = transactions.Sum(t => t.SoTien);
        var totalTransactions = transactions.Count;
        var daysInPeriod = (endDate - startDate).Days + 1;
        var averageDailySpending = daysInPeriod > 0 ? totalSpending / daysInPeriod : 0m;

        var categoryBreakdown = transactions
            .GroupBy(t => t.DanhMuc?.TenDanhMuc ?? "Không rõ")
            .Select(group => new
            {
                Category = group.Key,
                Amount = group.Sum(x => x.SoTien),
                Count = group.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var topTransactions = transactions
            .OrderByDescending(t => t.SoTien)
            .Take(3)
            .Select(t => $"- {t.NgayGiaoDich:dd/MM}: {FormatCurrency(t.SoTien)} VND ({t.DanhMuc?.TenDanhMuc ?? "Không rõ"}) {(string.IsNullOrWhiteSpace(t.MoTa) ? string.Empty : "- " + t.MoTa)}".Trim())
            .ToList();

        var payloadBuilder = new StringBuilder();
        payloadBuilder.AppendLine($"Khoảng thời gian: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}");
        payloadBuilder.AppendLine($"Tổng chi tiêu: {FormatCurrency(totalSpending)} VND trên {totalTransactions} giao dịch.");
        payloadBuilder.AppendLine($"Chi tiêu trung bình mỗi ngày: {FormatCurrency(averageDailySpending)} VND.");

        if (categoryBreakdown.Count > 0)
        {
            payloadBuilder.AppendLine("Top danh mục:");
            foreach (var category in categoryBreakdown.Take(5))
            {
                payloadBuilder.AppendLine($"- {category.Category}: {FormatCurrency(category.Amount)} VND ({category.Count} giao dịch)");
            }
        }

        if (topTransactions.Count > 0)
        {
            payloadBuilder.AppendLine("Giao dịch lớn:");
            foreach (var transaction in topTransactions)
            {
                payloadBuilder.AppendLine(transaction);
            }
        }

        var options = new ChatCompletionsOptions
        {
            Temperature = 0.3f,
            MaxTokens = 400,
            DeploymentName = _deploymentName,
        };

        options.Messages.Add(new ChatRequestSystemMessage(
            "Bạn là chuyên gia tài chính cá nhân. Hãy tạo báo cáo ngắn gọn bằng tiếng Việt với 2-3 gạch đầu dòng chính nêu tình hình chi tiêu và một đoạn khuyến nghị hành động cụ thể."));
        options.Messages.Add(new ChatRequestUserMessage(
            "Dưới đây là số liệu chi tiêu tháng hiện tại. Hãy phân tích và đưa ra khuyến nghị:\n" + payloadBuilder.ToString()));

        try
        {
            var response = await _client.GetChatCompletionsAsync(options, cancellationToken).ConfigureAwait(false);
            var answer = response.Value.Choices.FirstOrDefault()?.Message?.Content?.Trim();

            return string.IsNullOrWhiteSpace(answer)
                ? "Trợ lý chưa thể tạo báo cáo chi tiêu. Vui lòng thử lại sau."
                : answer;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Tạo báo cáo chi tiêu bằng AI thất bại.");
            return "Không thể gọi trợ lý AI để tạo báo cáo. Vui lòng thử lại sau.";
        }
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("N0", VietnameseCulture);
    }
}
