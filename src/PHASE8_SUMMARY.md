# Phase 8 Summary: Human-in-the-Loop (HITL) Verification

**Date**: February 2026  
**Status**: Complete  
**Phase**: Human Verification (Mandatory Gate)

---

## Overview

Phase 8 implements human verification of AI-extracted claim data. This is the legal gate that makes AI output trustworthy. The fundamental principle is: **AI assists, humans decide, and the system proves it.** Verification transfers accountability from the system to the human. Once this phase is complete, the system becomes operationally trustworthy because every piece of AI-generated data used for downstream processing has been explicitly reviewed and approved by a trained adjuster.

---

## What Was Built

### 1. Verification Policy Document
**File**: `docs/verification-policy.md`

Defines what verification means and why it is a legal responsibility:
- Verification transfers accountability from system to human
- AI hallucination risk demands explicit acceptance
- Verification is explicit, intentional, and auditable
- No silent acceptance, no auto-verify
- Per-field verification (stronger defensibility than batch)
- Verification is immutable (decisions cannot be changed)
- Preservation of AI output (corrections don't overwrite original)

### 2. VerificationRecord Domain Entity
**File**: `src/ClaimsIntake.Domain/Entities/VerificationRecord.cs`

Represents human verification decision:
- Immutable once created (verification decisions cannot be changed)
- Records who verified what and when (VerifiedBy, VerifiedAt)
- Captures action taken (Accepted, Corrected, Rejected)
- Stores corrected value if applicable (without overwriting AI output)
- Optional verification notes for context
- Validates action types and enforces business rules

### 3. VerificationRecord Repository
**Files**:
- `src/ClaimsIntake.Application/Interfaces/IVerificationRecordRepository.cs`
- `src/ClaimsIntake.Infrastructure/Persistence/VerificationRecordRepository.cs`

SQL persistence using Dapper:
- Insert verification records (no update operations - immutable)
- Query by ClaimId or ExtractedFieldId
- Check if verification exists for a field (single-action rule enforcement)
- No ORM magic - explicit SQL

### 4. Verification Command & Handler
**Files**:
- `src/ClaimsIntake.Application/Commands/VerifyExtractedFieldCommand.cs`
- `src/ClaimsIntake.Application/Handlers/VerifyExtractedFieldCommandHandler.cs`

Orchestrates verification workflow:
- Validates extracted field exists
- Enforces single-action rule (field can only be verified once)
- Validates field is in Unverified state
- Creates verification record
- Updates extracted field verification status
- Emits audit log for verification action
- Fails loudly if verification rules are violated

### 5. Audit Logging for Verification
**Updated Files**:
- `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

New audit method: `LogFieldVerifiedAsync`
- Records ClaimId, ExtractedFieldId, field name, action taken
- Captures actor (who performed verification)
- Provides audit trail for all verification decisions

### 6. Verification Guard Service
**Files**:
- `src/ClaimsIntake.Application/Services/IVerificationGuardService.cs`
- `src/ClaimsIntake.Infrastructure/Services/VerificationGuardService.cs`

Prevents downstream use of unverified data:
- `EnsureVerified()`: Validates single field is verified (throws if unverified)
- `EnsureAllVerifiedAsync()`: Validates all fields for claim are verified
- `GetVerifiedFieldsAsync()`: Returns only verified/corrected fields (filters out unverified/rejected)
- Fails loudly with clear error messages explaining verification requirement

### 7. API Endpoints
**Updated File**: `src/ClaimsIntake.API/Controllers/ClaimsController.cs`

New endpoints:
- `GET /api/claims/{claimId}/verification-queue` - Retrieve unverified fields pending verification (sorted by confidence, age, or field name)
- `POST /api/claims/extracted-fields/{extractedFieldId}/verify` - Verify an extracted field (human decision)

Existing endpoint enhanced:
- `GET /api/claims/{claimId}/extracted-fields` - Now returns all fields with verification status

---

## Key Design Decisions

### Per-Field Verification
Verification is performed per-field, not per-extraction batch. Each extracted field must be individually reviewed and verified. This provides stronger defensibility because it ensures the adjuster considers each piece of data independently. Trade-off: slower than batch verification, but stronger audit trail.

### Single-Action Rule
Each extracted field can only be verified once. Once an adjuster has taken an action (Accept, Correct, or Reject), that decision is final and immutable. If a correction is needed, a formal correction process with supervisor approval is required. This ensures verification decisions are not casually changed.

### Preservation of AI Output
When an adjuster corrects an AI-extracted value, the original AI output is preserved in the ExtractedFields table. The corrected value is stored in the VerificationRecords table. This dual recording ensures the audit trail shows both what the AI extracted and what the adjuster determined to be accurate.

### Verification Actions
Three explicit actions allowed:
- **Accepted**: AI value is accurate, mark as verified
- **Corrected**: AI value is incorrect, provide corrected value, mark as verified
- **Rejected**: AI value is incorrect and cannot be corrected, mark as rejected

No ambiguous actions. No "maybe" or "unsure". Adjuster must make a definitive decision.

### Verification Guard Service
Application layer guard that prevents downstream use of unverified data. Any code that attempts to use extracted field data must call the guard service to validate verification status. Fails loudly with clear error messages if unverified data is accessed.

### Verification Queue
Unverified fields are exposed through a dedicated endpoint that lists all fields pending verification. Queue can be sorted by confidence score (low-confidence first), age (oldest first), or field name (grouped). This helps adjusters prioritize review.

---

## What Was NOT Built (By Design)

### No Batch Verification
Verification is per-field only. Batch verification (verify all fields at once) is not supported because it weakens defensibility and accountability.

### No Auto-Verification
There is no automatic verification after a timeout or based on confidence score. All verification requires explicit human action.

### No Verification Editing
Once a field is verified, the decision cannot be changed through the API. Corrections require a formal process with supervisor approval (not implemented in this phase).

### No Role-Based Authorization Yet
The API endpoints accept any VerifiedBy value. Role-based access control (ensuring only authorized adjusters can verify) will be added in a future phase.

### No Verification UI
This phase implements the backend for verification. The UI for adjusters to review and verify fields is not included and will be built in a future phase.

---

## Compliance & Audit Readiness

### Audit Trail
Every verification action is logged with:
- ClaimId and ExtractedFieldId
- Field name and action taken
- Actor who performed verification
- Timestamp

### Accountability Chain
For any verified field, the system can trace:
1. Document upload (who, when)
2. AI extraction (model, prompt versions, confidence)
3. Human verification (who, when, action taken)
4. Corrected value if applicable (preserved alongside AI output)

### Legal Defensibility
The audit trail demonstrates:
- AI output was reviewed by a trained human
- Verification decision was explicit and intentional
- Original AI output was preserved (no silent overwrites)
- Accountability is personal (specific adjuster identity)

### Immutability
Verification records are immutable once created. No update operations exist. This ensures the audit trail accurately reflects the verification decision that was made at the time of review.

---

## Testing Validation (Manual)

To validate Phase 8 implementation:

1. **Upload document and extract data** (Phases 6-7)
2. **Retrieve verification queue**: `GET /api/claims/{claimId}/verification-queue`
3. **Verify field (Accept)**: `POST /api/claims/extracted-fields/{extractedFieldId}/verify` with ActionTaken="Accepted"
4. **Verify field (Correct)**: `POST /api/claims/extracted-fields/{extractedFieldId}/verify` with ActionTaken="Corrected" and CorrectedValue
5. **Verify field (Reject)**: `POST /api/claims/extracted-fields/{extractedFieldId}/verify` with ActionTaken="Rejected"
6. **Check audit logs**: Confirm FieldVerified events exist for each verification
7. **Test single-action rule**: Attempt to verify same field twice (should fail with clear error)
8. **Test guard service**: Attempt to use unverified field in downstream processing (should fail loudly)
9. **Verify preservation**: Confirm original AI value preserved in ExtractedFields, corrected value in VerificationRecords

---

## Downstream Integration Points

### Risk Assessment (Phase 9)
Risk assessment logic will use `IVerificationGuardService.GetVerifiedFieldsAsync()` to retrieve only verified fields. Unverified fields will be excluded from risk scoring.

### Claim Routing (Phase 10)
Routing logic will call `IVerificationGuardService.EnsureAllVerifiedAsync()` before routing a claim. If any fields are unverified, routing will fail with clear error message.

### Coverage Determination (Future)
Coverage logic will use guard service to ensure all relevant fields are verified before making coverage decisions.

---

## Next Steps (Phase 9 - Risk Assessment)

Phase 9 will implement risk assessment using verified AI data:
- Risk assessment service (rule-based + AI-assisted signals)
- RiskAssessment entity and repository
- Risk scoring logic (only uses verified fields)
- Explainability (which rules triggered, which AI signals contributed)
- Risk level assignment (Low, Medium, High, Critical)
- Audit logging for risk assessments
- API endpoints for triggering and retrieving risk assessments

---

## Files Created

### Documentation
- `docs/verification-policy.md`

### Domain Layer
- `src/ClaimsIntake.Domain/Entities/VerificationRecord.cs`

### Application Layer
- `src/ClaimsIntake.Application/Commands/VerifyExtractedFieldCommand.cs`
- `src/ClaimsIntake.Application/Handlers/VerifyExtractedFieldCommandHandler.cs`
- `src/ClaimsIntake.Application/Interfaces/IVerificationRecordRepository.cs`
- `src/ClaimsIntake.Application/Services/IVerificationGuardService.cs`

### Infrastructure Layer
- `src/ClaimsIntake.Infrastructure/Persistence/VerificationRecordRepository.cs`
- `src/ClaimsIntake.Infrastructure/Services/VerificationGuardService.cs`

### API Layer
- Updated: `src/ClaimsIntake.API/Controllers/ClaimsController.cs` (added verification endpoints)

### Services
- Updated: `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- Updated: `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

---

## Summary

Phase 8 establishes the legal gate for AI-assisted claims processing. AI output is treated as data, not truth. All extraction results must be explicitly reviewed and verified by a human before they can be used for downstream processing. Verification decisions are immutable, auditable, and attributable to specific adjusters. The system enforces verification requirements through guard services that fail loudly when unverified data is accessed.

**The system can now legitimately say: "AI assists, humans decide, and the system proves it."**

**Status**: Ready for Phase 9 (Risk Assessment)

---

**Document Owner**: Engineering Team  
**Last Updated**: February 2026  
**Phase**: 8 of N
