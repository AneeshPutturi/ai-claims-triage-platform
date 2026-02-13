// =============================================
// Service Implementation: ExtractionService
// Description: AI-assisted extraction with schema validation
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Text.Json;
using ClaimsIntake.Application.Services;
using NJsonSchema;

namespace ClaimsIntake.Infrastructure.Services;

/// <summary>
/// Extraction service using Azure OpenAI with strict schema validation.
/// AI output is data, not truth. All results are unverified by default.
/// </summary>
public class ExtractionService : IExtractionService
{
    private readonly IOpenAIService _openAIService;
    private readonly string _systemPrompt;
    private readonly string _userPromptTemplate;
    private readonly string _schemaJson;
    private readonly JsonSchema _schema;
    private const string SystemPromptVersion = "v1";
    private const string UserPromptVersion = "v1";
    private const string SchemaVersion = "v1";
    private const string ModelName = "gpt-4";

    public ExtractionService(
        IOpenAIService openAIService,
        string systemPromptPath,
        string userPromptTemplatePath,
        string schemaPath)
    {
        _openAIService = openAIService;
        
        // Load prompts and schema from files
        _systemPrompt = File.ReadAllText(systemPromptPath);
        _userPromptTemplate = File.ReadAllText(userPromptTemplatePath);
        _schemaJson = File.ReadAllText(schemaPath);
        _schema = JsonSchema.FromJsonAsync(_schemaJson).Result;
    }

    public async Task<ExtractionResult> ExtractFromDocumentAsync(
        Guid claimId,
        Guid documentId,
        string documentContent,
        CancellationToken cancellationToken = default)
    {
        // Build user prompt by injecting schema and document content
        var userPrompt = _userPromptTemplate
            .Replace("{schema}", _schemaJson)
            .Replace("{document_content}", documentContent);

        // Invoke OpenAI
        var response = await _openAIService.InvokeAsync(
            _systemPrompt,
            userPrompt,
            ModelName,
            cancellationToken);

        // Validate response against schema
        var validationErrors = _schema.Validate(response.Content);
        if (validationErrors.Any())
        {
            throw new InvalidOperationException(
                $"AI response does not conform to schema. Errors: {string.Join(", ", validationErrors.Select(e => e.ToString()))}");
        }

        // Parse JSON response
        var extractedData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.Content)
            ?? throw new InvalidOperationException("Failed to parse AI response as JSON");

        // Convert to ExtractedFieldData with confidence scores
        var fields = new List<ExtractedFieldData>();
        foreach (var kvp in extractedData)
        {
            if (kvp.Value.ValueKind != JsonValueKind.Null)
            {
                fields.Add(new ExtractedFieldData
                {
                    FieldName = kvp.Key,
                    FieldValue = kvp.Value.ToString(),
                    ConfidenceScore = CalculateConfidence(kvp.Key, kvp.Value)
                });
            }
        }

        return new ExtractionResult
        {
            Fields = fields,
            ModelName = response.ModelName,
            SystemPromptVersion = SystemPromptVersion,
            UserPromptVersion = UserPromptVersion,
            SchemaVersion = SchemaVersion,
            TokensUsed = response.TotalTokens,
            ExtractedAt = response.Timestamp
        };
    }

    /// <summary>
    /// Calculate confidence score based on field characteristics.
    /// This is a simple heuristic - can be enhanced with model-provided confidence.
    /// </summary>
    private decimal CalculateConfidence(string fieldName, JsonElement value)
    {
        // Base confidence
        decimal confidence = 0.85m;

        // Adjust based on field type and value characteristics
        if (fieldName == "lossDate" && value.ValueKind == JsonValueKind.String)
        {
            // Dates are typically high confidence if properly formatted
            if (DateTime.TryParse(value.GetString(), out _))
                confidence = 0.95m;
        }
        else if (fieldName == "estimatedDamageAmount" && value.ValueKind == JsonValueKind.Number)
        {
            // Numeric amounts are high confidence if present
            confidence = 0.90m;
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString() ?? "";
            // Longer text fields may have lower confidence
            if (text.Length > 200)
                confidence = 0.75m;
            else if (text.Length > 50)
                confidence = 0.80m;
        }

        return confidence;
    }
}
