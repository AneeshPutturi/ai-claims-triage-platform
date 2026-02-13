# Phase 9 Summary: Risk Assessment (Verified Data Only)

**Date**: February 2026  
**Status**: Complete  
**Phase**: Risk Assessment & Signaling

---

## Overview

Phase 9 implements risk assessment using verified data only. This phase demonstrates how to use AI responsibly in enterprise systems: rules first, AI second, humans always accountable. The fundamental principle is: **Risk is a signal, not a verdict.**

Risk assessment does not determine liability, does not approve or deny claims, and does not make financial decisions. Risk assessment informs humans about operational complexity and helps route claims to appropriate processing queues. False positives are acceptable; false negatives are dangerous.

---

## What Was Built

### 1. Risk Assessment Policy Document
**File**: `docs/risk-assessment-policy.md`

Defines what risk means and how it is evaluated:
- Risk is an operational prioritization signal, not a fraud score or verdict
- Three risk levels: Low, Medium, High (with operational outcomes)
- Deterministic rule categories (coverage consistency, field completeness, data inconsistency)
- AI signal scope (language ambiguity, unusual phrasing, narrative concerns - advisory only)
- Rule aggregation logic (binary rules with severity weights)
- Signal combination logic (rules override AI, AI can only escalate)
- Verified data requirement (unverified fields excluded)
- Explainability requirement (every risk level must be explainable)

### 2. RiskAssessment Domain Entity
**File**: `src/ClaimsIntake.Domain/Entities/RiskAssessment.cs`

Represents risk assessment snapshot:
- Immutable once created (historical snapshot)
- Stores risk level (Low, Medium, High)
- Separates rule signals and AI signals (never merged)
- Records overall score for sorting/analytics
- Tracks model used for AI observations
- Timestamped for audit trail

### 3. RiskAssessment Repository
**Files**:
- `src/ClaimsIntake.Application/Interfaces/IRiskAssessmentRepository.cs`
- `src/ClaimsIntake.Infrastructure/Persistence/RiskAssessmentRepository.cs`

SQL persistence using Dapper:
- Insert risk assessments (no update operations - immutable)
- Query latest assessment by ClaimId
- Query all assessments by ClaimId (historical view)
- Query by RiskAssessmentId
- No ORM magic - explicit SQL

### 4. Risk Evaluation Service
**Files**:
- `src/ClaimsIntake.Application/Services/IRiskEvaluationService.cs`
- `src/ClaimsIntake.Infrastructure/Services/RiskEvaluationService.cs`

Orchestrates risk assessment:
- Retrieves verified fields only (uses verification guard)
- Executes deterministic rules (coverage consistency, field completeness, data inconsistency, loss type coverage)
- Calculates rule-based risk level (binary rules with severity weights)
- Invokes AI for qualitative observations (advisory only)
- Combines rule and AI signals (rules override AI, AI can only escalate)
- Calculates overall score (0-100 scale)
- Returns structured result with explainability

### 5. Deterministic Rules Implemented
**Coverage Date Consistency** (Critical):
- Verifies extracted loss date falls within policy effective/expiration dates
- Mismatch triggers High Risk

**Critical Field Completeness** (Major):
- Verifies all mandatory fields (lossDate, lossLocation, lossType, lossDescription) are present and verified
- Missing fields trigger Medium/High Risk

**Data Inconsistency Detection** (Major):
- Compares verified extracted fields against original FNOL input
- Contradictions trigger Medium/High Risk

**Loss Type Coverage** (Critical):
- Verifies loss type is covered by policy
- Uncovered loss type triggers High Risk

### 6. AI Risk Observations
AI analyzes verified text fields to identify:
- Language ambiguity (vague or unclear descriptions)
- Unusual phrasing (inconsistent with typical claim narratives)
- Narrative concerns (inconsistent timelines, missing causal explanations)
- Completeness concerns (missing context or details)

AI is explicitly prohibited from:
- Assigning risk levels
- Making approval/denial recommendations
- Calculating fraud scores
- Determining liability
- Recommending payouts

AI output is tagged as "Advisory" and presented as context, not decisions.

### 7. Risk Evaluation Command & Handler
**Files**:
- `src/ClaimsIntake.Application/Commands/EvaluateRiskCommand.cs`
- `src/ClaimsIntake.Application/Handlers/EvaluateRiskCommandHandler.cs`

Orchestrates risk evaluation workflow:
- Validates claim exists
- Enforces verified-data-only requirement (fails loudly if unverified data detected)
- Invokes risk evaluation service
- Serializes rule and AI signals for persistence
- Creates risk assessment snapshot
- Persists to database
- Emits audit log

### 8. Audit Logging for Risk Assessment
**Updated Files**:
- `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

New audit method: `LogRiskAssessedAsync`
- Records ClaimId, risk level, rule trigger count, AI observation count
- Actor is "System" (automated but based on verified human-reviewed data)
- Provides audit trail for all risk assessments

### 9. API Endpoints
**Updated File**: `src/ClaimsIntake.API/Controllers/ClaimsController.cs`

New endpoints:
- `POST /api/claims/{claimId}/evaluate-risk` - Trigger risk assessment (verified data only)
- `GET /api/claims/{claimId}/risk-assessment` - Retrieve latest risk assessment with explainability

Response includes disclaimer: "This is a signal, not a decision. Human review required."

---

## Key Design Decisions

### Rules First, AI Second
Deterministic business rules are evaluated first and produce a rule-based risk level. AI observations are gathered second and can only escalate risk, never downgrade. If rules indicate High Risk, AI cannot override to Medium or Low.

### Verified Data Only
Risk assessment can only be performed on verified data. The verification guard service is invoked at the beginning of risk evaluation. If any required field is unverified, risk assessment fails loudly with an error message identifying the unverified fields.

### Separation of Rule and AI Signals
Rule-based signals and AI-generated signals are stored separately in the RiskAssessment table. They are never merged into a single score. This separation ensures transparency and allows adjusters to understand the basis for risk assessment.

### Binary Rules with Severity Weights
Rules are binary (triggered or not triggered) and have severity weights (Critical, Major, Minor). Aggregation logic is deterministic:
- Any Critical rule trigger → High Risk
- 2+ Major rule triggers → High Risk
- 1 Major or 3+ Minor triggers → Medium Risk
- No triggers → Low Risk (from rule perspective)

### AI Can Only Escalate
AI observations can escalate risk level but cannot downgrade:
- Rule-based High Risk → Final High Risk (AI cannot downgrade)
- Rule-based Medium Risk + critical AI concerns → Final High Risk
- Rule-based Low Risk + multiple AI concerns → Final Medium Risk

### Immutable Snapshots
Risk assessments are immutable once created. If claim circumstances change, a new risk assessment is performed and a new record is created. Historical assessments are preserved for audit purposes.

### Explainability
Every risk assessment includes:
- Which rules triggered and why
- What AI observations were made and which fields they relate to
- How signals were combined to determine final risk level
- Timestamp and model used

---

## What Was NOT Built (By Design)

### No Routing Yet
Risk assessment produces a risk level but does not route claims to processing queues. Routing logic will be implemented in Phase 10.

### No Automation Decisions
Risk assessment does not trigger automated actions. It informs humans, it does not replace them.

### No Financial Thresholds
Risk levels are based on data quality and consistency, not on claim amount. Financial thresholds for routing will be defined in future phases.

### No Fraud Scoring
Risk assessment is not a fraud score. It is an operational complexity signal. Fraud detection is a separate concern.

### No Coverage Determinations
Risk assessment does not determine whether a claim is covered. Coverage determination is a separate process performed by adjusters.

---

## Compliance & Audit Readiness

### Audit Trail
Every risk assessment is logged with:
- ClaimId and risk level
- Rule trigger count and AI observation count
- Timestamp
- Actor ("System" - automated but based on verified data)

### Explainability
For any risk assessment, the system can explain:
1. Which rules triggered and why
2. What AI observations were made
3. How signals were combined
4. Why the final risk level was assigned

### Verified Data Requirement
Risk assessment enforces the verified-data-only requirement through the verification guard service. Unverified data is excluded. This ensures risk assessment is based on human-reviewed information.

### Conservative Philosophy
Risk assessment is conservative. When in doubt, escalate. False positives are acceptable (over-scrutiny); false negatives are dangerous (under-scrutiny).

---

## Testing Validation (Manual)

To validate Phase 9 implementation:

1. **Upload document, extract, and verify data** (Phases 6-8)
2. **Trigger risk assessment**: `POST /api/claims/{claimId}/evaluate-risk`
3. **Retrieve risk assessment**: `GET /api/claims/{claimId}/risk-assessment`
4. **Review rule signals**: Confirm which rules triggered and why
5. **Review AI observations**: Confirm AI provided advisory observations only
6. **Check audit logs**: Confirm RiskAssessed event exists
7. **Test verified-data requirement**: Attempt risk assessment with unverified fields (should fail loudly)
8. **Test rule logic**: Create claims with different data patterns and verify risk levels match expectations
9. **Test AI escalation**: Verify AI can escalate but not downgrade risk levels

---

## Next Steps (Phase 10 - Triage & Routing)

Phase 10 will implement claim routing based on risk assessment:
- Routing service (assigns claims to processing queues based on risk level)
- Queue definitions (standard, experienced, senior adjuster queues)
- Routing rules (risk level → queue mapping)
- Claim state transition to "Triaged"
- Audit logging for routing decisions
- API endpoints for triggering routing and viewing queue assignments

---

## Files Created

### Documentation
- `docs/risk-assessment-policy.md`

### Domain Layer
- `src/ClaimsIntake.Domain/Entities/RiskAssessment.cs`

### Application Layer
- `src/ClaimsIntake.Application/Commands/EvaluateRiskCommand.cs`
- `src/ClaimsIntake.Application/Handlers/EvaluateRiskCommandHandler.cs`
- `src/ClaimsIntake.Application/Interfaces/IRiskAssessmentRepository.cs`
- `src/ClaimsIntake.Application/Services/IRiskEvaluationService.cs`

### Infrastructure Layer
- `src/ClaimsIntake.Infrastructure/Persistence/RiskAssessmentRepository.cs`
- `src/ClaimsIntake.Infrastructure/Services/RiskEvaluationService.cs`

### API Layer
- Updated: `src/ClaimsIntake.API/Controllers/ClaimsController.cs` (added risk assessment endpoints)

### Services
- Updated: `src/ClaimsIntake.Application/Services/IAuditLogService.cs`
- Updated: `src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs`

---

## Summary

Phase 9 demonstrates how to use AI responsibly in enterprise systems. Deterministic business rules are evaluated first and produce transparent, explainable outcomes. AI provides qualitative observations that enrich context but do not determine risk level. Rules always override AI. AI can only escalate risk, never downgrade. Risk assessment is based on verified data only, ensuring that human-reviewed information is the foundation for all risk signals.

**The system can now answer: "How do you use AI without letting it make decisions?"**
**Answer: Rules first, AI second, humans always accountable.**

**Status**: Ready for Phase 10 (Triage & Routing - The Final Push)

---

**Document Owner**: Engineering Team  
**Last Updated**: February 2026  
**Phase**: 9 of N
