# Human Verification Policy
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the policy for human verification of AI-extracted claim data. It establishes what verification means, who can perform it, what actions are allowed, and how verification decisions are recorded. The fundamental principle is: **AI assists, humans decide, and the system proves it.**

Verification is not a UI action. It is a legal responsibility. When an adjuster verifies AI-extracted data, they are asserting that the data is accurate and can be relied upon for downstream processing. This assertion carries legal and regulatory significance and must be recorded with full accountability.

---

## K8.1 – Verification Intent

Verification is the act of a human adjuster reviewing AI-extracted data and making an explicit decision about its accuracy. This decision transfers accountability from the system to the human. Before verification, the system is responsible for the accuracy of the data (and the system explicitly disclaims that responsibility by marking data as unverified). After verification, the human is responsible for the accuracy of the data.

AI hallucination risk demands explicit acceptance. AI systems can generate plausible-sounding but factually incorrect information. An AI might extract a claim amount that does not appear in the source document, or might misinterpret a date, or might identify a coverage type that is not mentioned. These errors can be subtle and difficult to detect. The verification process ensures that a trained human reviews the AI output and confirms its accuracy before it is used for downstream processing.

Verification is not optional. Unverified AI output cannot be used for coverage determinations, risk assessment, claim routing, or any other business decision. The system enforces this rule at the application layer. Any attempt to consume unverified data fails loudly with an error message explaining that verification is required.

Verification is explicit and intentional. There is no "approve all" button. There is no silent acceptance. There is no automatic verification after a timeout. Each extracted field must be individually reviewed and verified by a human. This per-field verification ensures that the adjuster considers each piece of data independently and does not rubber-stamp the entire extraction.

---

## K8.2 – Verification Actions

When reviewing an AI-extracted field, an adjuster can take one of three actions:

**Accept**: The adjuster confirms that the AI-extracted value is accurate and matches the source document. The extracted value is marked as verified and can be used for downstream processing. The original AI-extracted value is preserved in the ExtractedFields table. A verification record is created in the VerificationRecords table with ActionTaken="Accepted".

**Correct**: The adjuster determines that the AI-extracted value is incorrect or incomplete and provides a corrected value. The corrected value is stored in the VerificationRecords table. The original AI-extracted value is preserved in the ExtractedFields table and is not overwritten. The extracted field is marked as verified (because the human has reviewed it and provided the correct value). A verification record is created with ActionTaken="Corrected" and CorrectedValue populated.

**Reject**: The adjuster determines that the AI-extracted value is incorrect and cannot be corrected (for example, the field does not appear in the source document at all, or the AI extracted a value from the wrong section of the document). The extracted field is marked as rejected and is not used for downstream processing. A verification record is created with ActionTaken="Rejected" and optional VerificationNotes explaining why the field was rejected.

No ambiguous actions are allowed. There is no "maybe" or "unsure" action. The adjuster must make a definitive decision: Accept, Correct, or Reject. If the adjuster is uncertain, they should reject the field and escalate for review by a senior adjuster or supervisor.

---

## K8.3 – Verification Scope

Verification is performed per-field, not per-extraction batch. Each extracted field must be individually reviewed and verified. This per-field approach provides stronger defensibility because it ensures that the adjuster considers each piece of data independently.

The alternative approach (batch verification, where the adjuster verifies all fields from a document at once) is faster but less defensible. Batch verification creates a risk that the adjuster rubber-stamps the entire extraction without carefully reviewing each field. In a regulated environment where verification decisions may be scrutinized during audits or litigation, per-field verification provides a stronger audit trail.

Trade-offs:
- **Per-field verification**: Slower, more labor-intensive, but stronger defensibility and accountability. Each field has its own verification record with timestamp and actor.
- **Batch verification**: Faster, less labor-intensive, but weaker defensibility. A single verification record covers multiple fields, making it harder to trace accountability for individual data points.

This system uses per-field verification. Speed is secondary to defensibility in a regulated environment.

---

## K8.4 – Verification Immutability

Each extracted field can only be verified once. Once an adjuster has taken an action (Accept, Correct, or Reject), that decision is final and cannot be changed. This immutability ensures that the audit trail accurately reflects the verification decision that was made at the time of review.

If a verification decision needs to be corrected (for example, if the adjuster realizes they made a mistake), the correction process is:
1. The original verification record remains unchanged in the VerificationRecords table
2. A new audit log entry is created documenting the correction request
3. A supervisor or senior adjuster reviews the correction request
4. If approved, a new verification record is created with the corrected decision
5. The audit trail shows both the original decision and the correction, preserving full accountability

This process ensures that verification decisions are not casually changed and that all changes are documented and approved.

---

## K8.5 – Verification Accountability

Every verification action must be attributed to a specific human adjuster. The system records:
- **VerifiedBy**: The identity of the adjuster (user ID or email address)
- **VerifiedAt**: The timestamp when the verification was performed
- **ActionTaken**: The action the adjuster took (Accepted, Corrected, Rejected)
- **CorrectedValue**: If the action was Corrected, the corrected value provided by the adjuster
- **VerificationNotes**: Optional free-text notes explaining the verification decision

This accountability ensures that if a verification decision is questioned during an audit or dispute, the system can show exactly who made the decision and when. The adjuster's identity is traceable to a specific individual, not a generic system account or anonymous user.

---

## K8.6 – Preservation of AI Output

When an adjuster corrects an AI-extracted value, the original AI output is preserved. The ExtractedFields table contains the original AI-generated value. The VerificationRecords table contains the corrected value provided by the adjuster. This dual recording ensures that the audit trail shows both what the AI extracted and what the adjuster determined to be accurate.

This preservation is essential for:
- **AI performance analysis**: Understanding where the AI makes mistakes and improving model selection or prompt engineering
- **Audit defensibility**: Showing that the system did not silently overwrite AI output and that all corrections are explicitly documented
- **Legal protection**: Demonstrating that the insurer exercised due diligence in reviewing AI output and did not blindly accept AI-generated data

---

## K8.7 – Downstream Consumption Rules

Unverified AI output cannot be used for downstream processing. The application layer enforces this rule through guard clauses that check the VerificationStatus before allowing data to be consumed.

Any code that attempts to use extracted field data must:
1. Query the ExtractedFields table
2. Check the VerificationStatus column
3. If VerificationStatus is "Unverified", throw an exception with a clear error message
4. If VerificationStatus is "Verified", proceed with processing
5. If VerificationStatus is "Rejected", skip the field or handle appropriately

This enforcement ensures that unverified data never leaks into coverage determinations, risk assessments, claim routing, or other business decisions. The guard clause is explicit, loud, and impossible to bypass without intentionally removing the check.

---

## K8.8 – Verification Queue Management

Extracted fields pending verification are exposed through a read-only API endpoint that lists all unverified fields for a claim. This endpoint provides:
- Field name and AI-extracted value
- Confidence score (to help prioritize review)
- Document reference (to allow adjuster to view source document)
- Extraction timestamp (to identify stale extractions)

The verification queue can be sorted by:
- **Confidence score** (low-confidence fields first, as they are more likely to contain errors)
- **Extraction timestamp** (oldest first, to ensure timely review)
- **Field name** (to group similar fields together)

Adjusters use this queue to systematically review and verify extracted fields. The queue is read-only; verification actions are performed through a separate POST endpoint that enforces authorization and validation.

---

## K8.9 – Authorization and Access Control

Verification is a privileged action that requires specific authorization. Not all users can verify extracted fields. The system enforces role-based access control:
- **Adjusters**: Can verify extracted fields for claims assigned to them
- **Senior Adjusters**: Can verify extracted fields for any claim and can review correction requests
- **Supervisors**: Can verify extracted fields, review correction requests, and audit verification decisions
- **System Administrators**: Cannot verify extracted fields (verification is a business decision, not a technical action)

Authorization is enforced at the API layer through middleware that checks the user's role and permissions before allowing verification actions. Unauthorized verification attempts are rejected with a 403 Forbidden response.

---

## K8.10 – Audit Trail for Verification

Every verification action is recorded in the AuditLog table with:
- **Action**: "FieldVerified"
- **EntityType**: "ExtractedField"
- **EntityId**: ExtractedFieldId
- **Actor**: The adjuster's identity
- **Timestamp**: When the verification was performed
- **Outcome**: "Success" (or "Failure" if the verification was rejected due to validation errors)
- **Details**: JSON containing ClaimId, FieldName, ActionTaken, and optionally CorrectedValue

This audit trail provides a complete history of all verification decisions and can be queried to answer questions such as:
- Who verified this field?
- When was this field verified?
- What action did the adjuster take?
- If the field was corrected, what was the corrected value?
- How many fields has this adjuster verified today?
- What is the average time between extraction and verification?

---

## K8.11 – Verification Failure Scenarios

If an adjuster makes an incorrect correction (for example, corrects a date to the wrong value), the system does not silently fix the error. Instead:

1. The incorrect correction is preserved in the VerificationRecords table
2. The audit log shows the verification action with the incorrect correction
3. If the error is discovered later, a new audit log entry is created documenting the discovery
4. A supervisor reviews the error and determines the appropriate action
5. If a correction is needed, a new verification record is created with the correct value
6. The audit trail shows both the original incorrect correction and the subsequent correction, preserving full accountability

This process ensures that verification errors are not hidden and that all corrections are documented and approved. It also provides a learning opportunity to understand why the adjuster made the incorrect correction and to improve training or processes.

---

## K8.12 – Lock Verification Baseline

The verification behavior defined in this document is now locked. Future changes to verification rules, actions, or processes require legal review and formal approval. Casual refactoring of verification logic is prohibited.

Any proposed change to verification behavior must be documented with:
- **Justification**: Why is the change necessary?
- **Legal review**: Has the change been reviewed by legal counsel?
- **Impact analysis**: What verification records, audit logs, and application code will be affected?
- **Migration strategy**: How will existing verification records be handled?
- **Rollback plan**: How will the change be reversed if it causes problems?

Verification is a legal gate. Changes to this gate must be treated with the same rigor as changes to financial controls or regulatory compliance mechanisms.

---

## Related Documents

- **AI Extraction Policy**: `/docs/ai-extraction-policy.md`
- **Data Model**: `/docs/data-model.md`
- **Domain Model**: `/docs/domain-model.md`

---

**Document Owner**: Compliance & Legal  
**Last Updated**: February 2026  
**Next Review**: Q2 2026  
**Status**: LOCKED - Changes require legal approval
