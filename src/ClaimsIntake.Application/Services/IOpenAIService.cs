// =============================================
// Service Interface: IOpenAIService
// Description: Azure OpenAI client wrapper with managed identity
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for invoking Azure OpenAI with managed identity.
/// No OpenAI SDK calls outside this service.
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Invokes Azure OpenAI with system and user prompts.
    /// Returns the raw JSON response from the model.
    /// </summary>
    Task<OpenAIResponse> InvokeAsync(
        string systemPrompt,
        string userPrompt,
        string modelName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from Azure OpenAI invocation.
/// </summary>
public class OpenAIResponse
{
    public string Content { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public DateTime Timestamp { get; set; }
}
