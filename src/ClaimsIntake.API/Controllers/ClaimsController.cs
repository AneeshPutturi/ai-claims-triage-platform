// =============================================
// API Controller: ClaimsController
// Description: Claims endpoints for FNOL submission and retrieval
// Author: API Team
// Date: February 2026
// =============================================
// Purpose: HTTP endpoints with no business logic.
// Input validation enforced, no persistence logic in controller.
// =============================================

using Microsoft.AspNetCore.Mvc;
using ClaimsIntake.Application.Commands;
using ClaimsIntake.Application.Handlers;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly SubmitClaimCommandHandler _submitClaimHandler;
    private readonly ExtractClaimDataCommandHandler _extractClaimDataHandler;
    private readonly VerifyExtractedFieldCommandHandler _verifyFieldHandler;
    private readonly EvaluateRiskCommandHandler _evaluateRiskHandler;
    private readonly TriageClaimCommandHandler _triageClaimHandler;
    private readonly OverrideTriageCommandHandler _overrideTriageHandler;
    private readonly IClaimRepository _claimRepository;
    private readonly IExtractedFieldRepository _extractedFieldRepository;
    private readonly IRiskAssessmentRepository _riskAssessmentRepository;
    private readonly ITriageDecisionRepository _triageDecisionRepository;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        SubmitClaimCommandHandler submitClaimHandler,
        ExtractClaimDataCommandHandler extractClaimDataHandler,
        VerifyExtractedFieldCommandHandler verifyFieldHandler,
        EvaluateRiskCommandHandler evaluateRiskHandler,
        TriageClaimCommandHandler triageClaimHandler,
        OverrideTriageCommandHandler overrideTriageHandler,
        IClaimRepository claimRepository,
        IExtractedFieldRepository extractedFieldRepository,
        IRiskAssessmentRepository riskAssessmentRepository,
        ITriageDecisionRepository triageDecisionRepository,
        ILogger<ClaimsController> logger)
    {
        _submitClaimHandler = submitClaimHandler;
        _extractClaimDataHandler = extractClaimDataHandler;
        _verifyFieldHandler = verifyFieldHandler;
        _evaluateRiskHandler = evaluateRiskHandler;
        _triageClaimHandler = triageClaimHandler;
        _overrideTriageHandler = overrideTriageHandler;
        _claimRepository = claimRepository;
        _extractedFieldRepository = extractedFieldRepository;
        _riskAssessmentRepository = riskAssessmentRepository;
        _triageDecisionRepository = triageDecisionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Submit First Notice of Loss (FNOL)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubmitClaimResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitClaim(
        [FromBody] SubmitClaimRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting claim for policy {PolicyNumber}", request.PolicyNumber);

        var command = new SubmitClaimCommand(
            PolicyNumber: request.PolicyNumber,
            LossDate: request.LossDate,
            LossType: request.LossType,
            LossLocation: request.LossLocation,
            LossDescription: request.LossDescription ?? string.Empty,
            SubmittedBy: request.SubmittedBy);

        var claimId = await _submitClaimHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Claim submitted successfully: {ClaimId}", claimId);

        var response = new SubmitClaimResponse(claimId);
        return CreatedAtAction(nameof(GetClaim), new { id = claimId }, response);
    }

    /// <summary>
    /// Retrieve claim by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaim(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving claim {ClaimId}", id);

        var claim = await _claimRepository.GetByIdAsync(id, cancellationToken);

        if (claim == null)
        {
            _logger.LogWarning("Claim not found: {ClaimId}", id);
            return NotFound(new ErrorResponse("Claim not found", $"No claim exists with ID {id}"));
        }

        var response = new ClaimResponse(
            ClaimId: claim.ClaimId,
            ClaimNumber: claim.ClaimNumber.Value,
            PolicyNumber: claim.PolicyNumber.Value,
            LossDate: claim.LossDate.Value,
            LossType: claim.LossType,
            LossLocation: claim.LossLocation,
            LossDescription: claim.LossDescription,
            Status: claim.Status.ToString(),
            CreatedAt: claim.CreatedAt,
            UpdatedAt: claim.UpdatedAt,
            SubmittedBy: claim.SubmittedBy);

        return Ok(response);
    }

    /// <summary>
    /// Trigger AI extraction for a claim document
    /// </summary>
    [HttpPost("{claimId}/documents/{documentId}/extract")]
    [ProducesResponseType(typeof(ExtractClaimDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtractClaimData(
        Guid claimId,
        Guid documentId,
        [FromBody] ExtractClaimDataRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extracting data from document {DocumentId} for claim {ClaimId}", documentId, claimId);

        var command = new ExtractClaimDataCommand
        {
            ClaimId = claimId,
            DocumentId = documentId,
            Actor = request.Actor
        };

        var result = await _extractClaimDataHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Extraction completed: {Message}", result.Message);

        var response = new ExtractClaimDataResponse(
            result.Success,
            result.Message,
            result.ExtractedFieldIds);

        return Ok(response);
    }

    /// <summary>
    /// Retrieve unverified extracted fields for a claim
    /// </summary>
    [HttpGet("{claimId}/extracted-fields")]
    [ProducesResponseType(typeof(ExtractedFieldsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExtractedFields(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving extracted fields for claim {ClaimId}", claimId);

        var fields = await _extractedFieldRepository.GetByClaimIdAsync(claimId, cancellationToken);

        var response = new ExtractedFieldsResponse(
            fields.Select(f => new ExtractedFieldDto(
                f.ExtractedFieldId,
                f.FieldName,
                f.FieldValue,
                f.ConfidenceScore,
                f.VerificationStatus.ToString(),
                f.ExtractedAt,
                f.ExtractedByModel)).ToList());

        return Ok(response);
    }

    /// <summary>
    /// Retrieve unverified extracted fields pending verification (verification queue)
    /// </summary>
    [HttpGet("{claimId}/verification-queue")]
    [ProducesResponseType(typeof(VerificationQueueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVerificationQueue(
        Guid claimId,
        [FromQuery] string? sortBy = "confidence",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving verification queue for claim {ClaimId}", claimId);

        var fields = await _extractedFieldRepository.GetByClaimIdAsync(claimId, cancellationToken);

        // Filter to unverified fields only
        var unverifiedFields = fields
            .Where(f => f.VerificationStatus == ClaimsIntake.Domain.Enums.VerificationStatus.Unverified)
            .Select(f => new VerificationQueueItemDto(
                f.ExtractedFieldId,
                f.DocumentId,
                f.FieldName,
                f.FieldValue,
                f.ConfidenceScore,
                f.ExtractedAt,
                f.ExtractedByModel))
            .ToList();

        // Sort based on query parameter
        unverifiedFields = sortBy?.ToLower() switch
        {
            "confidence" => unverifiedFields.OrderBy(f => f.ConfidenceScore).ToList(),
            "age" => unverifiedFields.OrderBy(f => f.ExtractedAt).ToList(),
            "field" => unverifiedFields.OrderBy(f => f.FieldName).ToList(),
            _ => unverifiedFields.OrderBy(f => f.ConfidenceScore).ToList()
        };

        var response = new VerificationQueueResponse(unverifiedFields);

        return Ok(response);
    }

    /// <summary>
    /// Verify an extracted field (human decision)
    /// </summary>
    [HttpPost("extracted-fields/{extractedFieldId}/verify")]
    [ProducesResponseType(typeof(VerifyFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyExtractedField(
        Guid extractedFieldId,
        [FromBody] VerifyFieldRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Verifying extracted field {ExtractedFieldId} with action {ActionTaken} by {VerifiedBy}",
            extractedFieldId,
            request.ActionTaken,
            request.VerifiedBy);

        var command = new VerifyExtractedFieldCommand
        {
            ExtractedFieldId = extractedFieldId,
            VerifiedBy = request.VerifiedBy,
            ActionTaken = request.ActionTaken,
            CorrectedValue = request.CorrectedValue,
            VerificationNotes = request.VerificationNotes
        };

        var result = await _verifyFieldHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Verification completed: {Message}", result.Message);

        var response = new VerifyFieldResponse(
            result.Success,
            result.Message,
            result.VerificationId);

        return Ok(response);
    }

    /// <summary>
    /// Evaluate risk for a claim (verified data only)
    /// </summary>
    [HttpPost("{claimId}/evaluate-risk")]
    [ProducesResponseType(typeof(EvaluateRiskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateRisk(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evaluating risk for claim {ClaimId}", claimId);

        var command = new EvaluateRiskCommand
        {
            ClaimId = claimId
        };

        var result = await _evaluateRiskHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Risk evaluation completed: {Message}", result.Message);

        var response = new EvaluateRiskResponse(
            result.Success,
            result.Message,
            result.RiskAssessmentId,
            result.RiskLevel,
            result.OverallScore);

        return Ok(response);
    }

    /// <summary>
    /// Retrieve risk assessment for a claim
    /// </summary>
    [HttpGet("{claimId}/risk-assessment")]
    [ProducesResponseType(typeof(RiskAssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRiskAssessment(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving risk assessment for claim {ClaimId}", claimId);

        var riskAssessment = await _riskAssessmentRepository.GetLatestByClaimIdAsync(claimId, cancellationToken);

        if (riskAssessment == null)
        {
            _logger.LogWarning("Risk assessment not found for claim {ClaimId}", claimId);
            return NotFound(new ErrorResponse("Risk assessment not found", $"No risk assessment exists for claim {claimId}"));
        }

        var response = new RiskAssessmentResponse(
            riskAssessment.RiskAssessmentId,
            riskAssessment.ClaimId,
            riskAssessment.RiskLevel.ToString(),
            riskAssessment.RuleSignals,
            riskAssessment.AISignals,
            riskAssessment.OverallScore,
            riskAssessment.CreatedAt,
            riskAssessment.AssessedByModel,
            "This is a signal, not a decision. Human review required.");

        return Ok(response);
    }

    /// <summary>
    /// Triage claim to appropriate processing queue
    /// </summary>
    [HttpPost("{claimId}/triage")]
    [ProducesResponseType(typeof(TriageClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriageClaim(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Triaging claim {ClaimId}", claimId);

        var command = new TriageClaimCommand
        {
            ClaimId = claimId
        };

        var result = await _triageClaimHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Triage completed: {Message}", result.Message);

        var response = new TriageClaimResponse(
            result.Success,
            result.Message,
            result.TriageDecisionId,
            result.Queue);

        return Ok(response);
    }

    /// <summary>
    /// Override triage routing decision (requires authorization)
    /// </summary>
    [HttpPost("{claimId}/triage/override")]
    [ProducesResponseType(typeof(OverrideTriageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OverrideTriage(
        Guid claimId,
        [FromBody] OverrideTriageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Overriding triage for claim {ClaimId} to queue {Queue} by {OverrideBy}",
            claimId,
            request.Queue,
            request.OverrideBy);

        var command = new OverrideTriageCommand
        {
            ClaimId = claimId,
            Queue = request.Queue,
            OverrideBy = request.OverrideBy,
            OverrideReason = request.OverrideReason
        };

        var result = await _overrideTriageHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Triage override completed: {Message}", result.Message);

        var response = new OverrideTriageResponse(
            result.Success,
            result.Message,
            result.TriageDecisionId,
            result.Queue);

        return Ok(response);
    }

    /// <summary>
    /// Retrieve triage history for a claim
    /// </summary>
    [HttpGet("{claimId}/triage-history")]
    [ProducesResponseType(typeof(TriageHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTriageHistory(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving triage history for claim {ClaimId}", claimId);

        var decisions = await _triageDecisionRepository.GetAllByClaimIdAsync(claimId, cancellationToken);

        var response = new TriageHistoryResponse(
            decisions.Select(d => new TriageDecisionDto(
                d.TriageDecisionId,
                d.RiskAssessmentId,
                d.Queue,
                d.RoutedAt,
                d.IsOverride,
                d.OverrideBy,
                d.OverrideReason)).ToList());

        return Ok(response);
    }

    /// <summary>
    /// Retrieve claims by triage queue
    /// </summary>
    [HttpGet("queue/{queue}")]
    [ProducesResponseType(typeof(QueueClaimsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClaimsByQueue(
        string queue,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving claims for queue {Queue}", queue);

        var decisions = await _triageDecisionRepository.GetByQueueAsync(queue, cancellationToken);

        var claimIds = decisions.Select(d => d.ClaimId).ToList();
        var claims = new List<QueueClaimDto>();

        foreach (var claimId in claimIds)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId, cancellationToken);
            if (claim != null)
            {
                claims.Add(new QueueClaimDto(
                    claim.ClaimId,
                    claim.ClaimNumber.Value,
                    claim.LossType,
                    claim.Status.ToString(),
                    claim.CreatedAt));
            }
        }

        var response = new QueueClaimsResponse(queue, claims);

        return Ok(response);
    }
}

// Request/Response DTOs
public record SubmitClaimRequest(
    string PolicyNumber,
    DateTime LossDate,
    string LossType,
    string LossLocation,
    string? LossDescription,
    string SubmittedBy);

public record SubmitClaimResponse(Guid ClaimId);

public record ClaimResponse(
    Guid ClaimId,
    string ClaimNumber,
    string PolicyNumber,
    DateTime LossDate,
    string LossType,
    string LossLocation,
    string LossDescription,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string SubmittedBy);

public record ErrorResponse(string Error, string Message, string? CorrelationId = null);

public record ExtractClaimDataRequest(string Actor);

public record ExtractClaimDataResponse(
    bool Success,
    string Message,
    List<Guid> ExtractedFieldIds);

public record ExtractedFieldsResponse(List<ExtractedFieldDto> Fields);

public record ExtractedFieldDto(
    Guid ExtractedFieldId,
    string FieldName,
    string? FieldValue,
    decimal ConfidenceScore,
    string VerificationStatus,
    DateTime ExtractedAt,
    string ExtractedByModel);

public record VerificationQueueResponse(List<VerificationQueueItemDto> UnverifiedFields);

public record VerificationQueueItemDto(
    Guid ExtractedFieldId,
    Guid DocumentId,
    string FieldName,
    string? FieldValue,
    decimal ConfidenceScore,
    DateTime ExtractedAt,
    string ExtractedByModel);

public record VerifyFieldRequest(
    string VerifiedBy,
    string ActionTaken,
    string? CorrectedValue = null,
    string? VerificationNotes = null);

public record VerifyFieldResponse(
    bool Success,
    string Message,
    Guid VerificationId);

public record EvaluateRiskResponse(
    bool Success,
    string Message,
    Guid RiskAssessmentId,
    string RiskLevel,
    decimal OverallScore);

public record RiskAssessmentResponse(
    Guid RiskAssessmentId,
    Guid ClaimId,
    string RiskLevel,
    string RuleSignals,
    string AISignals,
    decimal OverallScore,
    DateTime CreatedAt,
    string? AssessedByModel,
    string Disclaimer);

public record TriageClaimResponse(
    bool Success,
    string Message,
    Guid TriageDecisionId,
    string Queue);

public record OverrideTriageRequest(
    string Queue,
    string OverrideBy,
    string OverrideReason);

public record OverrideTriageResponse(
    bool Success,
    string Message,
    Guid TriageDecisionId,
    string Queue);

public record TriageHistoryResponse(List<TriageDecisionDto> Decisions);

public record TriageDecisionDto(
    Guid TriageDecisionId,
    Guid RiskAssessmentId,
    string Queue,
    DateTime RoutedAt,
    bool IsOverride,
    string? OverrideBy,
    string? OverrideReason);

public record QueueClaimsResponse(string Queue, List<QueueClaimDto> Claims);

public record QueueClaimDto(
    Guid ClaimId,
    string ClaimNumber,
    string LossType,
    string Status,
    DateTime CreatedAt);
