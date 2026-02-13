# Triage & Routing Policy
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the policy for claim triage and routing. It establishes what triage means, how routing decisions are made, and how routing outcomes are recorded. The fundamental principle is: **Routing is operational, not legal. Routing prioritizes human effort, it does not automate outcomes.**

Triage is the process of assigning claims to appropriate processing queues based on risk assessment. Routing decisions are deterministic, explainable, and based on verified risk assessments. Routing never changes claim data, never determines liability, and never approves or denies claims. Routing simply directs claims to the right humans for review.

---

## K10.1 – Purpose of Triage (Business Definition)

Triage is the operational process of prioritizing claims for adjuster review. It answers the question: "Which queue should this claim enter?" Triage does not answer: "Should this claim be approved?" or "How much should we pay?" Triage is about speed and focus, not about outcomes.

Speed matters in claims operations. Policyholders expect timely responses. Regulators require prompt investigation. Delays create dissatisfaction, increase costs, and expose the insurer to regulatory penalties. Triage ensures that claims are routed to the appropriate processing queue quickly and consistently.

Focus matters in claims operations. Adjusters have limited time and attention. High-risk claims require senior adjuster expertise and thorough investigation. Low-risk claims can be processed through streamlined workflows. Triage ensures that adjuster attention is allocated efficiently based on claim complexity and risk.

Triage is not approval or denial. A claim routed to the "Auto-Review" queue is not automatically approved. A claim routed to the "Manual Investigation" queue is not automatically denied. Routing determines which humans review the claim and how much scrutiny is applied. The humans make the final decisions.

Triage is not permanent. Claims can be re-routed if circumstances change (new documents uploaded, additional information provided, adjuster escalation). Routing decisions are recorded as immutable snapshots, but new routing decisions can be made based on updated risk assessments or human override.

---

## K10.2 – Triage Queues

The system defines three logical processing queues:

**Auto-Review Queue**: Claims that are straightforward, well-documented, and low-risk. These claims can be processed through streamlined workflows with standard adjuster oversight. Auto-review does not mean automatic approval; it means the claim can be reviewed efficiently without extensive investigation.

Operational characteristics:
- Assigned to any available adjuster
- Standard processing time (e.g., 24-48 hours)
- No supervisor review required unless claim amount exceeds threshold
- Streamlined documentation requirements

**Standard Review Queue**: Claims that have minor inconsistencies, missing optional fields, or qualitative concerns that warrant additional review. These claims require experienced adjuster review and may require additional documentation.

Operational characteristics:
- Assigned to experienced adjusters
- Extended processing time (e.g., 3-5 business days)
- Supervisor review recommended
- Additional documentation may be requested

**Manual Investigation Queue**: Claims that have significant inconsistencies, contradictions, or red flags that require senior adjuster investigation. These claims require thorough review, supervisor approval, and potentially legal or compliance review.

Operational characteristics:
- Assigned to senior adjusters only
- Extended processing time (e.g., 7-14 business days)
- Supervisor review mandatory
- Legal or compliance review may be required
- Additional investigation and documentation required

Queue names are logical, not physical. The actual implementation may use different names or additional sub-queues based on operational needs. The important principle is that queues represent different levels of scrutiny and adjuster expertise.

---

## K10.3 – Routing Rules (Deterministic Mapping)

Routing decisions are based on deterministic rules that map risk levels to triage queues. No AI is involved in routing. Routing is a simple lookup based on the risk assessment outcome.

**Routing rules**:
- Risk Level = Low → Auto-Review Queue
- Risk Level = Medium → Standard Review Queue
- Risk Level = High → Manual Investigation Queue

These rules are deterministic and transparent. Given a risk level, the routing outcome is always the same. There is no probabilistic logic, no AI inference, no hidden complexity. The rules can be expressed as a simple decision table or if-then-else logic.

Additional routing factors may be considered in future phases (claim amount thresholds, policy type, jurisdiction, claimant history), but the core routing logic is based on risk level. Risk level is the primary routing signal because it captures data quality, consistency, and complexity.

---

## K10.4 – Routing Immutability

Routing decisions are recorded as immutable snapshots. Once a claim is routed to a queue, that routing decision is preserved in the TriageDecision table. If the claim is re-routed (due to updated risk assessment or human override), a new TriageDecision record is created. The original routing decision is never modified or deleted.

This immutability ensures that the audit trail accurately reflects the routing history. If a claim is audited months or years later, the audit trail shows exactly when the claim was routed, to which queue, and based on which risk assessment. This traceability is essential for understanding claim processing timelines and for defending routing decisions during disputes.

Multiple routing decisions for the same claim are allowed. The most recent routing decision determines the current queue assignment, but historical routing decisions are preserved for audit purposes.

---

## K10.5 – Latest Risk Assessment Rule

Routing always uses the most recent risk assessment snapshot. If multiple risk assessments exist for a claim (due to updated information or re-evaluation), the routing logic queries for the latest assessment by timestamp and uses that assessment to determine the routing outcome.

This latest-only rule ensures that routing reflects the current understanding of the claim's risk profile. If new information changes the risk assessment, the routing should reflect that change. But the historical routing decisions are preserved to show how the claim's risk profile evolved over time.

---

## K10.6 – Routing Does Not Re-Evaluate Risk

The routing handler retrieves the latest risk assessment and applies routing rules. It does not re-evaluate risk. Risk evaluation is a separate process (Phase 9) that must be explicitly triggered. Routing assumes that a risk assessment exists and is current.

If no risk assessment exists for a claim, routing fails loudly with an error message. Routing cannot proceed without a risk assessment because the risk level is the primary routing signal. This fail-loud behavior ensures that claims are not routed arbitrarily or based on default assumptions.

---

## K10.7 – Human Override Capability

Authorized users (supervisors, senior adjusters) can override routing decisions. An override allows a human to manually assign a claim to a different queue than the one determined by the routing rules.

Overrides are recorded as new TriageDecision records with a flag indicating that the decision was a human override. The override record includes:
- The original routing decision (preserved, not modified)
- The new queue assignment
- The actor who performed the override
- The reason for the override (required, free-text)
- Timestamp

Overrides do not alter the risk assessment. The risk assessment remains unchanged. The override only changes the queue assignment. This separation ensures that risk assessment and routing are independent concerns and that overrides are transparent and auditable.

---

## K10.8 – Override Justification Requirement

Every override must include a reason. The reason is a free-text field that explains why the human decided to override the routing rules. This justification is required for accountability and for understanding override patterns.

Common override reasons might include:
- "Claimant is high-value customer, escalating for priority handling"
- "Similar claim was recently litigated, requires legal review"
- "Adjuster has domain expertise in this loss type, assigning directly"
- "Risk assessment is outdated due to new information, re-routing pending re-evaluation"

The reason is stored in the TriageDecision record and is visible in the audit trail. This transparency ensures that overrides are not arbitrary and that patterns in override behavior can be analyzed to improve routing rules or risk assessment logic.

---

## K10.9 – Idempotency and Duplicate Prevention

Routing is idempotent. If a routing command is issued for a claim that has already been routed (based on the same risk assessment), the system returns the existing routing decision rather than creating a duplicate. This prevents accidental duplicate routing and ensures that the audit trail is clean.

Idempotency is enforced by checking whether a TriageDecision record already exists for the claim with the same RiskAssessmentId. If it exists, the existing decision is returned. If it does not exist, a new decision is created.

Re-routing is allowed if the risk assessment has changed (new RiskAssessmentId) or if a human override is issued. But duplicate routing based on the same risk assessment is prevented.

---

## K10.10 – Claim State Transition

When a claim is successfully routed, its status is updated to "Triaged". This state transition indicates that the claim has completed the intake and triage process and is ready for adjuster review.

The state transition is atomic with the routing decision. If routing fails, the claim status is not updated. If routing succeeds, the claim status is updated to "Triaged" and the transition is recorded in the audit log.

Valid state transitions to "Triaged":
- Submitted → Triaged (if risk assessment and routing complete)
- Validated → Triaged (if risk assessment and routing complete)
- Verified → Triaged (if risk assessment and routing complete)

Invalid state transitions:
- Triaged → Triaged (already triaged, idempotency enforced)
- Closed → Triaged (claim is closed, cannot re-triage)

---

## K10.11 – Routing Audit Trail

Every routing decision is logged in the AuditLog table with:
- **Action**: "ClaimTriaged"
- **EntityType**: "TriageDecision"
- **EntityId**: TriageDecisionId
- **Actor**: "System" (for rule-based routing) or human identity (for overrides)
- **Timestamp**: When the routing was performed
- **Outcome**: "Success" or "Failure"
- **Details**: JSON containing ClaimId, RiskAssessmentId, Queue, IsOverride, OverrideReason (if applicable)

This audit trail provides a complete history of all routing decisions and can be queried to analyze routing patterns, queue workload distribution, and override behavior.

---

## K10.12 – Routing Explainability

Every routing decision is explainable. The TriageDecision record includes:
- `RiskAssessmentId`: Which risk assessment was used for routing
- `Queue`: Which queue the claim was assigned to
- `RoutedAt`: When the routing was performed
- `IsOverride`: Whether the routing was a human override
- `OverrideReason`: Why the override was performed (if applicable)
- `OverrideBy`: Who performed the override (if applicable)

An adjuster or auditor reviewing a routing decision can understand exactly why the claim was assigned to a particular queue. They can trace back to the risk assessment, review the rule triggers and AI observations, and understand the routing logic.

---

## K10.13 – Routing Philosophy

Routing is deterministic and explainable. Given a risk level, the routing outcome is always the same. There are no hidden algorithms, no black-box logic, no unexplainable decisions.

Routing is operational, not legal. Routing determines which humans review the claim, not whether the claim is approved or denied. Routing prioritizes human effort based on claim complexity and risk.

Routing is based on verified risk assessments. Risk assessments are based on verified data. Verified data is based on human review of AI output. This chain of accountability ensures that routing decisions are grounded in human-reviewed information, not in raw AI hallucinations.

Routing can be overridden by humans. Overrides are transparent, auditable, and require justification. This flexibility ensures that routing rules do not become rigid constraints that prevent appropriate handling of unusual claims.

Routing is a signal, not a verdict. It informs humans about where to focus their attention. It does not replace human judgment.

---

## Related Documents

- **Risk Assessment Policy**: `/docs/risk-assessment-policy.md`
- **Verification Policy**: `/docs/verification-policy.md`
- **AI Extraction Policy**: `/docs/ai-extraction-policy.md`
- **Data Model**: `/docs/data-model.md`
- **Domain Model**: `/docs/domain-model.md`

---

**Document Owner**: Operations & Compliance  
**Last Updated**: February 2026  
**Next Review**: Q2 2026  
**Status**: LOCKED - Changes require operational approval
