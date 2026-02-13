// =============================================
// Service Implementation: OpenAIService
// Description: Azure OpenAI client wrapper with managed identity
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using ClaimsIntake.Application.Services;

namespace ClaimsIntake.Infrastructure.Services;

/// <summary>
/// Azure OpenAI service using managed identity.
/// No OpenAI SDK calls outside this service.
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;

    public OpenAIService(string endpoint, string deploymentName)
    {
        // Use managed identity for authentication
        _client = new AzureOpenAIClient(
            new Uri(endpoint),
            new DefaultAzureCredential());
        
        _deploymentName = deploymentName;
    }

    public async Task<OpenAIResponse> InvokeAsync(
        string systemPrompt,
        string userPrompt,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        var chatClient = _client.GetChatClient(_deploymentName);

        var messages = new[]
        {
            new ChatMessage(ChatMessageRole.System, systemPrompt),
            new ChatMessage(ChatMessageRole.User, userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.0f, // Deterministic output
            MaxTokens = 2000
        };

        var response = await chatClient.CompleteChatAsync(messages, options, cancellationToken);

        return new OpenAIResponse
        {
            Content = response.Value.Content[0].Text,
            ModelName = modelName,
            PromptTokens = response.Value.Usage.InputTokenCount,
            CompletionTokens = response.Value.Usage.OutputTokenCount,
            TotalTokens = response.Value.Usage.TotalTokenCount,
            Timestamp = DateTime.UtcNow
        };
    }
}
