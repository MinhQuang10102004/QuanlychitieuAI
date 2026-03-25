using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyChiTieu.Services;

public class AiService
{
    private readonly ChatClient _client;
    private readonly ILogger<AiService> _logger;

    public AiService(OpenAI.OpenAIClient openAIClient, IConfiguration configuration, ILogger<AiService> logger)
    {
        _client = openAIClient.GetChatClient("gpt-3.5-turbo");
        _logger = logger;
    }

    public async Task<string> ClassifyTransactionAsync(string description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Không có mô tả giao dịch để phân loại.";
        }

        try
        {
            var response = await _client.CompleteChatAsync(
                new System.Collections.Generic.List<ChatMessage>
                {
                    new SystemChatMessage("Bạn là trợ lý tài chính. Hãy phân loại giao dịch vào một trong các danh mục chi tiêu phổ biến như ăn uống, hóa đơn, đi lại, giải trí, y tế, giáo dục, mua sắm, tiết kiệm."),
                    new UserChatMessage($"Mô tả giao dịch: \"{description}\". Hãy trả về tên danh mục ngắn gọn.")
                }, 
                new ChatCompletionOptions { Temperature = 0.2f, MaxOutputTokenCount = 128 },
                cancellationToken
            ).ConfigureAwait(false);

            var answer = response.Value.Content.FirstOrDefault()?.Text?.Trim();

            return string.IsNullOrWhiteSpace(answer)
                ? "Không nhận được phản hồi từ trợ lý AI."
                : answer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Phân loại giao dịch bằng AI thất bại.");
            return "Không thể phân loại giao dịch ngay lúc này. Vui lòng thử lại sau.";
        }
    }

    public async Task<string?> ChatAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return "Vui lòng nhập câu hỏi.";

        const int maxAttempts = 2;
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                _logger.LogInformation("AI Chat - attempt {Attempt} promptLen={Len}", attempt, Math.Min(prompt?.Length ?? 0, 1024));
                var response = await _client.CompleteChatAsync(
                    new System.Collections.Generic.List<ChatMessage>
                    {
                        new SystemChatMessage("Bạn là trợ lý tài chính hữu ích. Trả lời gọn, dễ hiểu và đưa ví dụ khi cần."),
                        new UserChatMessage(prompt)
                    },
                    new ChatCompletionOptions { Temperature = 0.6f, MaxOutputTokenCount = 512 },
                    cancellationToken
                ).ConfigureAwait(false);

                var answer = response.Value.Content.FirstOrDefault()?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(answer))
                {
                    _logger.LogWarning("AI returned empty answer on attempt {Attempt}.", attempt);
                    return null;
                }
                _logger.LogInformation("AI returned answer (len {Len}) on attempt {Attempt}.", answer.Length, attempt);
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI chat attempt {Attempt} failed.", attempt);
                if (attempt >= maxAttempts)
                {
                    _logger.LogWarning("AI chat failed after {Attempts} attempts.", attempt);
                    return null;
                }
                // Simple backoff
                await Task.Delay(TimeSpan.FromSeconds(1 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
