# Phase 7 Summary: Azure OpenAI Integration - Structured Extraction

**Date**: February 2026  
**Status**: Complete  
**Phase**: AI-Assisted Extraction (Unverified)

---

## Overview

Phase 7 introduces AI-assisted extraction of structured claim data from unstructured documents using Azure OpenAI. This phase implements strict controls around AI invocation, schema validation, prompt versioning, and audit logging. The fundamental principle is: **AI output is data, not truth.** All extraction results are marked as unverified by default and require human review before being used for downstream processing.

---

## What Was Built

### 1. Canonical JSON Schema
**File**: `ai/schemas/claim-extraction-v1.json`

Defines the exact structure for AI extraction output:
- All fields optional (AI returns null for unknown values)
- Flat structure (no nested free-form objects)
- Allowed fields: lossDate, lossLocation, lossType, lossDescription, propertyAddress, estimatedDamageAmount, contactPhone, contactEmail, involvedParties
- Explicitly excluded: payout amounts, liability determinations, coverage decisions, approval status, risk scores, fraud indicators

### 2. System Prompt (Non-Negotiable Rules)
**File**: `ai/prompts/system-prompt-v1.txt`

Establishes strict boundaries for AI behavior:
- Return ONLY valid JSON (no prose, no explanation)
- Use null for unknown values (no guessing or inference)
- Extract only factual information (no calculations or determinations)
- No financial decisions, liability assessments, or approval decisions

### 3. User Prompt Template
**File**: `ai/prompts/user-prompt-template-v1.txt`

Deterministic template with placeholders:
- `{schema}`: JSON schema definition (injected at runtime)
- `{document_content}`: Text content from document
- No examples or ambiguous language

### 4. OpenAI Service Wrapper
**Files**: 
- `src/ClaimsIntake.Application/Services/IOpenAIService.cs`
- `src/ClaimsIntake.Infrastructure/Services/OpenAIService.cs`

Azure OpenAI client wrapper with managed identity:
- No OpenAI SDK calls outside this service
- Uses DefaultAzureCredential for authentication
- Temperature set to 0.0 for deterministic output
- Returns raw JSON response with token usage metadata

### 5. Extraction Service
**Files**:
- `src/ClaimsIntake.Application/Services/IExtractionService.cs`
- `src/ClaimsIntake.Infrastructure/Services/ExtractionService.cs`

Orchestrates AI extraction with validation:
- Loads prompts and schema from files
- Injects schema and document content into user prompt
- Invokes OpenAI service
- Validates response against JSON schema (rejects invalid responses)
- Calculates confidence scores for each extracted field
- Returns extraction results with metadata (model name, prompt versions, schema version, token usage)

### 6. ExtractedField Domain Entity
**File**: `src/ClaimsIntake.Domain/Entities/ExtractedField.cs`

Represents AI-extracted data:
- Immutable once created
- Stores field name, value, confidence score
- Tracks verification status (Unverified, Verified, Corrected, Rejected)
- Records extraction metadata (model name, prompt versions, schema version)
- State transitions controlled (can only move from Unverified to Verified/Corrected/Rejected)

### 7. ExtractedField Repository
**Files**:
- `src/ClaimsIntake.Application/Interfaces/IExtractedFieldRepository.cs`
- `src/ClaimsIntake.Infrastructure/Persistence/ExtractedFieldRepository.cs`

SQL persistence using Dapper:
- Insert extracted fields
- Query by ClaimId or DocumentId
- Update verification status
- No ORM magic - explicit SQL

### 8. Extraction Command & Handler
**Files**:
- `src/ClaimsIntake.Application/Commands/ExtractClaimDataCommand.cs`
- `src/ClaimsIntake.Application/Handlers/ExtractClaimDataCommandHandler.cs`

Orchestrates extraction workflow:
- Validates claim and document exist
- Checks for existing extraction (idempotency)
- Downloads document content from blob storage
- Invokes extraction service
- Persists extracted fields to database
- Emits audit log for AI invocation

### 9. Audit Logging for AI
**Updated Files**:
- `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

New audit method: `LogAIExtractionPerformedAsync`
- Records ClaimId, DocumentId, model name, tokens used, fields extracted
- Captures actor (who triggered extraction)
- Provides audit trail for all AI invocations

### 10. API Endpoints
**Updated File**: `src/ClaimsIntake.API/Controllers/ClaimsController.cs`

New endpoints:
- `POST /api/claims/{claimId}/documents/{documentId}/extract` - Trigger AI extraction
- `GET /api/claims/{claimId}/extracted-fields` - Retrieve unverified extracted fields

---

## Key Design Decisions

### AI Output is Data, Not Truth
All extraction results are marked as `VerificationStatus = Unverified` by default. They remain in this state until a human adjuster reviews and verifies them. Unverified data is visible for review but never used for automated decision-making.

### Schema Validation is Mandatory
Every AI response is validated against the JSON schema before persistence. Invalid responses are rejected and logged. No attempt is made to "fix" malformed output. This ensures that only well-formed data enters the system.

### Prompt Versioning for Audit Trail
System prompt, user prompt template, and schema are all versioned. Every extraction result stores the versions used, allowing auditors to understand exactly what instructions were given to the AI model.

### Idempotency
The extraction handler checks for existing extractions before invoking AI. If extraction already exists for a document, it returns the existing results rather than re-extracting. This prevents duplicate AI invocations and wasted tokens.

### Confidence Scores
Each extracted field includes a confidence score (0.0 to 1.0). This is currently calculated using simple heuristics based on field type and value characteristics. Can be enhanced with model-provided confidence in future versions.

### Separation of Concerns
AI extraction results are stored in a separate table (ExtractedFields), not in the Claims table. This ensures that AI-generated outputs never contaminate the authoritative claim record without human review.

---

## What Was NOT Built (By Design)

### No Automatic Verification
AI extraction results are not automatically marked as verified. Human review is required for all extracted data before it can be used for downstream processing.

### No Retry Logic Yet
If AI invocation fails due to transient errors, the handler does not automatically retry. Retry logic can be added in future phases with exponential backoff and circuit breaker patterns.

### No Batch Extraction
Extraction is performed one document at a time. Batch extraction across multiple documents can be added in future phases if needed.

### No Model Selection Logic
The extraction service uses a single model (gpt-4) with fixed parameters. Model selection based on document type or complexity can be added in future phases.

### No Verification UI
This phase implements the backend for extraction and storage. The UI for human verification of extracted fields is not included and will be built in a future phase.

---

## Compliance & Audit Readiness

### Audit Trail
Every AI invocation is logged with:
- ClaimId and DocumentId
- Model name and token usage
- Number of fields extracted
- Actor who triggered extraction
- Timestamp

### Prompt Traceability
Every extraction result stores:
- System prompt version
- User prompt version
- Schema version

This allows auditors to reconstruct exactly what instructions were given to the AI for any extraction.

### Schema Immutability
Schema version v1 is locked. Future schema changes require a new version (v2, v3, etc.) with backward compatibility requirements.

### Explainability
Confidence scores provide context for human review. Low-confidence extractions can be prioritized for manual review or escalated for additional scrutiny.

---

## Testing Validation (Manual)

To validate Phase 7 implementation:

1. **Upload a claim document** (Phase 6 functionality)
2. **Trigger extraction**: `POST /api/claims/{claimId}/documents/{documentId}/extract`
3. **Verify extraction results**: `GET /api/claims/{claimId}/extracted-fields`
4. **Check audit logs**: Confirm AIExtractionPerformed event exists
5. **Verify schema validation**: Attempt extraction with malformed document (should fail gracefully)
6. **Test idempotency**: Trigger extraction twice for same document (should return existing results)

---

## Next Steps (Phase 8 - Human Verification)

Phase 8 will implement human verification workflows:
- Verification UI for adjusters to review extracted fields
- Verification command and handler
- VerificationRecords table persistence
- Audit logging for verification actions
- State transitions from Unverified to Verified/Corrected/Rejected
- Integration with claim lifecycle (transition to Verified state after all fields verified)

---

## Files Created

### AI Assets
- `ai/schemas/claim-extraction-v1.json`
- `ai/prompts/system-prompt-v1.txt`
- `ai/prompts/user-prompt-template-v1.txt`

### Application Layer
- `src/ClaimsIntake.Application/Services/IOpenAIService.cs`
- `src/ClaimsIntake.Application/Services/IExtractionService.cs`
- `src/ClaimsIntake.Application/Commands/ExtractClaimDataCommand.cs`
- `src/ClaimsIntake.Application/Handlers/ExtractClaimDataCommandHandler.cs`
- `src/ClaimsIntake.Application/Interfaces/IExtractedFieldRepository.cs`

### Domain Layer
- `src/ClaimsIntake.Domain/Entities/ExtractedField.cs`

### Infrastructure Layer
- `src/ClaimsIntake.Infrastructure/Services/OpenAIService.cs`
- `src/ClaimsIntake.Infrastructure/Services/ExtractionService.cs`
- `src/ClaimsIntake.Infrastructure/Persistence/ExtractedFieldRepository.cs`

### API Layer
- Updated: `src/ClaimsIntake.API/Controllers/ClaimsController.cs` (added extraction endpoints)

### Services
- Updated: `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- Updated: `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

---

## Summary

Phase 7 establishes the foundation for AI-assisted extraction with strict controls, schema validation, prompt versioning, and audit logging. AI output is treated as data, not truth. All extraction results are unverified by default and require human review. The system is designed for a regulated environment where AI must be explainable, auditable, and defensible.

**Status**: Ready for Phase 8 (Human Verification)

---

**Document Owner**: Engineering Team  
**Last Updated**: February 2026  
**Phase**: 7 of N
