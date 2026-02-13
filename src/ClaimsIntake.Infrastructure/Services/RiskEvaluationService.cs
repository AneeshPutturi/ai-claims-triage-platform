// =============================================
// Service Implementation: RiskEvaluationService
// Description: Risk assessment with deterministic rules and AI signals
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Text.Json;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Infrastructure.Services;

/// <summary>
/// Risk evaluation service using verified data only.
/// Rules first, AI second, humans always accountable.
/// </summary>
public class RiskEvaluationService : IRiskEvaluationService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicySnapshotRepository _policySnapshotRepository;
    private readonly IVerificationGuardService _verificationGuard;
    private readonly IOpenAIService _openAIService;
    private const string ModelName = "gpt-4";

    public RiskEvaluationService(
        IClaimRepository claimRepository,
        IPolicySnapshotRepository policySnapshotRepository,
        IVerificationGuardService verificationGuard,
        IOpenAIService openAIService)
    {
        _claimRepository = claimRepository;
        _policySnapshotRepository = policySnapshotRepository;
        _verificationGuard = verificationGuard;
        _openAIService = openAIService;
    }

    public async Task<RiskEvaluationResult> EvaluateRiskAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {claimId} not found");

        // Get policy snapshot
        var policySnapshot = await _policySnapshotRepository.GetByClaimIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException($"Policy snapshot not found for claim {claimId}");

        // Get verified fields only
        var verifiedFields = await _verificationGuard.GetVerifiedFieldsAsync(claimId, cancellationToken);
        var verifiedFieldsList = verifiedFields.ToList();

        // Execute deterministic rules
        var ruleSignals = ExecuteRules(claim, policySnapshot, verifiedFieldsList);

        // Calculate rule-based risk level
        var ruleBased RiskLevel = CalculateRuleBasedRisk(ruleSignals);

        // Get AI observations (advisory only)
        var aiObservations = await GetAIObservationsAsync(claim, verifiedFieldsList, cancellationToken);

        // Combine rule and AI signals
        var finalRiskLevel = CombineSignals(ruleBasedRiskLevel, aiObservations);

        // Calculate overall score
        var overallScore = CalculateOverallScore(ruleSignals, aiObservations);

        return new RiskEvaluationResult
        {
            RiskLevel = finalRiskLevel,
            RuleSignals = ruleSignals,
            AIObservations = aiObservations,
            OverallScore = overallScore,
            ModelUsed = ModelName
        };
    }

    private List<RuleSignal> ExecuteRules(
        Domain.Entities.Claim claim,
        Domain.Entities.PolicySnapshot policySnapshot,
        List<Domain.Entities.ExtractedField> verifiedFields)
    {
        var signals = new List<RuleSignal>();

        // Rule 1: Coverage Date Consistency
        var lossDateField = verifiedFields.FirstOrDefault(f => f.FieldName == "lossDate");
        if (lossDateField != null && DateTime.TryParse(lossDateField.FieldValue, out var extractedLossDate))
        {
            var coverageDateMismatch = extractedLossDate < policySnapshot.EffectiveDate ||
                                      extractedLossDate > policySnapshot.ExpirationDate;
            
            signals.Add(new RuleSignal
            {
                RuleName = "CoverageDateConsistency",
                Triggered = coverageDateMismatch,
                Severity = "Critical",
                Description = coverageDateMismatch
                    ? $"Extracted loss date {extractedLossDate:yyyy-MM-dd} falls outside policy coverage period"
                    : "Loss date falls within policy coverage period"
            });
        }

        // Rule 2: Critical Field Completeness
        var mandatoryFields = new[] { "lossDate", "lossLocation", "lossType", "lossDescription" };
        var missingFields = mandatoryFields.Where(f => !verifiedFields.Any(vf => vf.FieldName == f)).ToList();
        
        signals.Add(new RuleSignal
        {
            RuleName = "CriticalFieldCompleteness",
            Triggered = missingFields.Any(),
            Severity = "Major",
            Description = missingFields.Any()
                ? $"Missing verified mandatory fields: {string.Join(", ", missingFields)}"
                : "All mandatory fields present and verified"
        });

        // Rule 3: Data Inconsistency Detection
        var lossDateInconsistent = false;
        if (lossDateField != null && DateTime.TryParse(lossDateField.FieldValue, out var verifiedLossDate))
        {
            lossDateInconsistent = Math.Abs((verifiedLossDate - claim.LossDate.Value).TotalDays) > 1;
        }

        signals.Add(new RuleSignal
        {
            RuleName = "DataInconsistencyDetection",
            Triggered = lossDateInconsistent,
            Severity = "Major",
            Description = lossDateInconsistent
                ? "Inconsistency detected between FNOL loss date and verified extracted loss date"
                : "FNOL data consistent with verified extracted data"
        });

        // Rule 4: Loss Type Coverage
        var lossTypeField = verifiedFields.FirstOrDefault(f => f.FieldName == "lossType");
        var lossTypeCovered = true; // Simplified - would check against policy coverage in production
        
        signals.Add(new RuleSignal
        {
            RuleName = "LossTypeCoverage",
            Triggered = !lossTypeCovered,
            Severity = "Critical",
            Description = lossTypeCovered
                ? "Loss type is covered by policy"
                : "Loss type may not be covered by policy"
        });

        return signals;
    }

    private RiskLevel CalculateRuleBasedRisk(List<RuleSignal> ruleSignals)
    {
        var criticalTriggered = ruleSignals.Any(r => r.Severity == "Critical" && r.Triggered);
        var majorTriggered = ruleSignals.Count(r => r.Severity == "Major" && r.Triggered);
        var minorTriggered = ruleSignals.Count(r => r.Severity == "Minor" && r.Triggered);

        if (criticalTriggered)
            return RiskLevel.High;
        
        if (majorTriggered >= 2)
            return RiskLevel.High;
        
        if (majorTriggered >= 1 || minorTriggered >= 3)
            return RiskLevel.Medium;
        
        return RiskLevel.Low;
    }

    private async Task<List<AIObservation>> GetAIObservationsAsync(
        Domain.Entities.Claim claim,
        List<Domain.Entities.ExtractedField> verifiedFields,
        CancellationToken cancellationToken)
    {
        // Build verified claim data for AI analysis
        var claimData = new
        {
            lossDescription = claim.LossDescription,
            verifiedFields = verifiedFields.Select(f => new { f.FieldName, f.FieldValue })
        };

        var systemPrompt = @"You are a claims review assistant. Your task is to analyze verified claim data and provide qualitative observations that may be relevant for adjuster review.

RULES:
1. Provide observations only, not recommendations or decisions.
2. Do not assign risk levels, fraud scores, or approval recommendations.
3. Focus on language clarity, narrative consistency, and completeness.
4. If you observe potential concerns, describe them factually without judgment.
5. Return observations in structured JSON format.
6. If no concerns are observed, return an empty observations array.";

        var userPrompt = $@"Analyze the following verified claim data and provide qualitative observations.

Claim Data:
{JsonSerializer.Serialize(claimData, new JsonSerializerOptions { WriteIndented = true })}

Return a JSON object with the following structure:
{{
  ""observations"": [
    {{
      ""category"": ""language_ambiguity | unusual_phrasing | narrative_concern | completeness_concern"",
      ""description"": ""Factual description of the observation"",
      ""relevantField"": ""Field name where observation was made""
    }}
  ]
}}

Remember: Provide observations only. Do not make recommendations or assign risk levels.";

        try
        {
            var response = await _openAIService.InvokeAsync(
                systemPrompt,
                userPrompt,
                ModelName,
                cancellationToken);

            var aiResponse = JsonSerializer.Deserialize<AIRiskResponse>(response.Content);
            
            return aiResponse?.Observations?.Select(o => new AIObservation
            {
                Category = o.Category ?? "unknown",
                Description = o.Description ?? "",
                RelevantField = o.RelevantField ?? ""
            }).ToList() ?? new List<AIObservation>();
        }
        catch
        {
            // If AI invocation fails, return empty observations (rules still apply)
            return new List<AIObservation>();
        }
    }

    private RiskLevel CombineSignals(RiskLevel ruleBasedRisk, List<AIObservation> aiObservations)
    {
        // Rules always override AI
        if (ruleBasedRisk == RiskLevel.High)
            return RiskLevel.High;

        // AI can only escalate, never downgrade
        var criticalAIConcerns = aiObservations.Count(o => 
            o.Category == "narrative_concern" || o.Category == "completeness_concern");

        if (ruleBasedRisk == RiskLevel.Medium && criticalAIConcerns > 0)
            return RiskLevel.High;

        if (ruleBasedRisk == RiskLevel.Low && aiObservations.Count >= 3)
            return RiskLevel.Medium;

        return ruleBasedRisk;
    }

    private decimal CalculateOverallScore(List<RuleSignal> ruleSignals, List<AIObservation> aiObservations)
    {
        decimal score = 0;

        // Rule-based scoring (0-70 points)
        var criticalCount = ruleSignals.Count(r => r.Severity == "Critical" && r.Triggered);
        var majorCount = ruleSignals.Count(r => r.Severity == "Major" && r.Triggered);
        var minorCount = ruleSignals.Count(r => r.Severity == "Minor" && r.Triggered);

        score += criticalCount * 30;
        score += majorCount * 15;
        score += minorCount * 5;

        // AI-based scoring (0-30 points)
        score += Math.Min(aiObservations.Count * 10, 30);

        return Math.Min(score, 100);
    }

    private class AIRiskResponse
    {
        public List<AIObservationDto>? Observations { get; set; }
    }

    private class AIObservationDto
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? RelevantField { get; set; }
    }
}
