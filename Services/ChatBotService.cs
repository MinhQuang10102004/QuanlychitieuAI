using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyChiTieu.Services;

public class ChatBotService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<ChatBotService> _logger;

    public ChatBotService(OpenAIClient client, IConfiguration configuration, ILogger<ChatBotService> logger)
    {
        _client = client;
        _logger = logger;
        _deploymentName = configuration["OpenAI:AdvisorDeployment"]
            ?? configuration["OpenAI:DeploymentName"]
            ?? throw new InvalidOperationException("Chưa cấu hình tên deployment cho OpenAI. Vui lòng đặt OpenAI:DeploymentName hoặc OpenAI:AdvisorDeployment.");
    }

    public async Task<string> GetFinancialAdviceAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Vui lòng nhập câu hỏi để được tư vấn.";
        }

        var options = new ChatCompletionsOptions
        {
            Temperature = 0.4f,
            MaxTokens = 256,
            DeploymentName = _deploymentName,
        };

        options.Messages.Add(new ChatRequestSystemMessage(
            "Bạn là cố vấn tài chính cá nhân, đưa ra lời khuyên ngắn gọn, thực tế dựa trên nguyên tắc quản lý chi tiêu thông minh."));
        options.Messages.Add(new ChatRequestUserMessage(query));

        try
        {
            var response = await _client.GetChatCompletionsAsync(options, cancellationToken).ConfigureAwait(false);
            var answer = response.Value.Choices.FirstOrDefault()?.Message?.Content?.Trim();

            return string.IsNullOrWhiteSpace(answer)
                ? "Trợ lý chưa thể phản hồi. Vui lòng thử lại sau."
                : answer;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Chatbot tài chính gọi OpenAI thất bại.");
            return "Không thể kết nối tới trợ lý AI ngay lúc này. Vui lòng thử lại sau.";
        }
    }
}
