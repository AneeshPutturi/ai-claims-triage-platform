# Domain Model
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the core domain concepts that govern the AI-Driven Commercial Claims Intake & Triage Platform. It establishes a shared vocabulary and conceptual framework that guides all subsequent design and implementation decisions. These concepts are expressed in plain language, independent of any technical implementation, and serve as the authoritative reference for what the system does and does not represent.

---

## K1.1 – What a Claim Is

A Claim, in this system, represents a formal request for insurance coverage related to a specific, time-bound loss event. A claim is always tied to a specific policy and a specific loss date. It is the legal and operational artifact that initiates the claims process and triggers the insurer's obligation to investigate, validate, and respond to a policyholder's request for coverage.

A claim is not a payment. It is not a settlement. It is not a coverage determination or an underwriting decision. A claim is not a complaint, a question, or a general inquiry. A claim is not a renewal notice, a policy change request, or a billing inquiry. A claim is specifically and exclusively a request for coverage related to a loss event that occurred on or before the date of submission.

Every claim must reference a specific policy and a specific loss date. Without these two anchors, the system cannot determine whether coverage was in force at the time of loss, and therefore cannot proceed with processing. A claim without a policy reference or a loss date is incomplete and cannot be accepted into the system. The claim represents the beginning of a formal investigation process, and it carries legal and regulatory significance that demands precision in its definition and handling.

---

## K1.2 – First Notice of Loss (FNOL) Concept

The First Notice of Loss is the legal trigger that initiates claim processing. It is the moment when the insurer first receives formal notification of a loss event from a policyholder or authorized representative. FNOL is not the same as a complete claim investigation; it is the minimal but accurate notification that a loss has occurred and that the policyholder is requesting coverage.

FNOL data must be minimal but accurate. The system requires only essential information at this stage: policy number, loss date, loss type, loss location, and basic description of the loss. Supporting documents may be attached, but FNOL does not require a complete investigation file. The purpose of FNOL is to establish the legal trigger for the insurer's duty to respond, not to resolve the claim. Minimal data at FNOL stage means the system can acknowledge receipt quickly, establish a claim record, and begin the investigation process without requiring the policyholder to provide exhaustive documentation upfront.

However, minimal does not mean inaccurate. The policy number must be valid and resolvable. The loss date must be a specific calendar date, not a range or estimate. The loss type must be recognizable within the insurer's coverage taxonomy. The loss location must be specific enough to determine jurisdiction and coverage applicability. Accuracy at FNOL stage is critical because it determines whether the claim can proceed to validation and whether coverage can be verified. A claim with an invalid policy number or an ambiguous loss date cannot proceed, and the system must reject it clearly and immediately.

FNOL is distinct from full claim processing. FNOL establishes the legal trigger and creates the claim record. Full claim processing includes investigation, document review, risk assessment, and triage routing. The system's role at FNOL stage is to accept minimal but accurate data, validate basic completeness, and establish the claim record. It is not to investigate the claim, determine coverage, or make any coverage decisions. Those activities occur in subsequent stages.

---

## K1.3 – Claim Lifecycle States

A claim progresses through a series of well-defined lifecycle states, each representing a distinct stage of processing and each requiring specific actions before transition to the next state. These states are: Submitted, Validated, Verified, and Triaged.

**Submitted** is the initial state when a claim is first received through any intake channel. At this stage, the system has accepted the claim data but has not yet performed any validation or verification. The claim exists in the system, but it is not yet cleared for processing. Submitted claims may contain incomplete data, invalid policy references, or missing required fields. The system performs only basic syntactic validation at this stage—checking that required fields are present and that data types are correct. A claim remains in Submitted state until it passes validation checks.

**Validated** is the state after the system has confirmed that the claim contains accurate, complete, and resolvable data. Policy number has been verified against the policy system. Loss date is a valid calendar date. Loss type is recognized within the coverage taxonomy. Loss location is specific and resolvable. All required fields are present and correctly formatted. A claim in Validated state has passed automated checks and is ready for human review. However, Validated does not mean the claim is approved or that coverage has been determined. It means the claim data is accurate and complete enough to proceed to the next stage.

**Verified** is the state after a human adjuster has reviewed the extracted and validated claim data and confirmed its accuracy. The adjuster has examined the supporting documents, confirmed that extracted information matches the source documents, corrected any discrepancies, and flagged any suspicious inconsistencies. A claim in Verified state has been reviewed by a human and is trusted for downstream processing. Verified is the point at which AI-generated outputs transition from unverified to trusted. Only after human verification does extracted data become authoritative input for risk assessment and triage routing.

**Triaged** is the final state in the intake and triage process. The claim has been assigned a risk level, routed to the appropriate queue, and is ready for assignment to an adjuster or investigation team. A claim in Triaged state has completed the intake and triage workflow and is ready for the next phase of processing. Triaged claims may be routed to standard processing queues, expedited queues, or escalation queues depending on risk assessment results.

Transitions between states must be controlled and auditable. A claim cannot skip states; it must progress sequentially from Submitted to Validated to Verified to Triaged. Each transition must be triggered by a specific action or validation result. Each transition must be recorded in the audit log with timestamp, user, and reason. The system must prevent invalid transitions and must provide clear error messages when a transition cannot be completed. Auditability of transitions is essential for regulatory defensibility and for understanding the processing history of any claim.

---

## K1.4 – Coverage-in-Force Validation

Coverage-in-Force validation is the process of determining whether the policy provided by the claimant was active and provided coverage for the loss type on the specific date the loss occurred. This validation is point-in-time, not current-state. The system must determine coverage status as of the loss date, not as of the current date.

This distinction is critical. A policy may have been in force on the loss date but may have expired, been cancelled, or been replaced by the time the claim is submitted. Conversely, a policy may have been inactive on the loss date but may be active today. The system must validate coverage against the historical state of the policy on the loss date, not against the current state of the policy. This requires access to policy history, effective dates, expiration dates, and coverage details as they existed on the loss date.

If a claim references a policy that was not in force on the loss date, the claim must be rejected at the validation stage. A claim without valid coverage should not consume downstream processing resources. The system must provide a clear rejection reason: "Policy was not in force on loss date" or "Coverage did not apply to loss type on loss date." This rejection is not a coverage denial; it is a determination that the claim does not meet the basic eligibility requirement for processing. The claim record is created and the rejection is logged, but the claim does not proceed to verification or triage.

Coverage-in-Force validation is performed early in the workflow, immediately after basic data validation. It is a deterministic check against policy data, not a judgment call. Either the policy was in force on the loss date or it was not. Either the coverage applied to the loss type or it did not. This validation is essential for preventing invalid claims from consuming adjuster time and for ensuring that only eligible claims proceed to investigation.

---

## K1.5 – Policy Snapshot

A Policy Snapshot is a point-in-time record of the policy's coverage status, effective dates, expiration dates, and coverage details as they existed on the loss date. The snapshot is captured and persisted independently of the external policy system. It is not a reference to the current policy record; it is a historical record of what the policy looked like on a specific date.

The Policy Snapshot must include EffectiveDate (the date the policy became active), ExpirationDate (the date the policy expired or was replaced), coverage limits, deductibles, covered loss types, and any exclusions or restrictions that applied on the loss date. The snapshot is created at the time the claim is validated and is stored as part of the claim record. It is never updated or modified, even if the external policy system is updated or corrected.

This approach ensures historical defensibility. If a claim is audited months or years after submission, the audit trail shows exactly what coverage information was available and what coverage determination was made based on that information. If the external policy system is corrected or updated, the claim record still reflects the coverage status that was known at the time of processing. This prevents retroactive changes to coverage determinations and ensures that claims are always evaluated based on the information available at the time of processing.

The Policy Snapshot is persisted independently because external policy systems may be updated, corrected, or replaced. The snapshot ensures that the claim record is self-contained and does not depend on the continued availability or accuracy of external systems. If the external policy system is decommissioned or migrated, the claim record still contains the policy information that was used to make coverage determinations. This independence is essential for long-term auditability and for defending coverage decisions during regulatory review or litigation.

---

## K1.6 – Human Verification

Human verification is the mandatory process by which a claims adjuster reviews all AI-generated outputs and confirms their accuracy before those outputs become trusted input for downstream processing. AI outputs are untrusted by default. They are explicitly marked as unverified and cannot be used for any decision-making or processing until a human has reviewed and confirmed them.

This approach protects the business from the risk of AI hallucination—the phenomenon where AI systems generate plausible-sounding but factually incorrect information. An AI system might extract a claim amount that does not appear in the source document, or might summarize a loss description in a way that misrepresents the actual loss, or might identify a coverage type that is not actually mentioned in the claim. These errors might be subtle and difficult to detect, but they can lead to incorrect coverage determinations, invalid claim routing, or downstream processing errors.

Human verification requires an adjuster to examine the source documents and compare them to the AI-extracted information. The adjuster confirms that extracted fields match the source documents, that summaries accurately represent the loss, and that no information has been hallucinated or misrepresented. The adjuster corrects any discrepancies and flags any suspicious inconsistencies. Only after this human review does the extracted data transition from unverified to verified and become authoritative input for downstream processing.

The principle is explicit and non-negotiable: AI never has final authority. AI accelerates work by pre-organizing and extracting information, but humans make the final determination of accuracy. This principle is embedded in the workflow design, in the data model, and in the audit trail. Every piece of AI-generated information is marked with its verification status. Every claim record shows which information has been verified by a human and which information remains unverified. This transparency ensures that downstream users understand the provenance of the information they are working with and can make informed decisions about how much confidence to place in it.

---

## K1.7 – Risk Concept

Risk, in this system, is a routing signal that indicates the likelihood that a claim requires escalated investigation or manual review. Risk is not a fraud verdict. Risk is not a coverage determination. Risk is not a claim approval or denial. Risk is a signal that helps the system route claims to the appropriate processing queue based on the characteristics of the claim and the potential for complications.

Risk assessment combines deterministic business rules with AI-assisted qualitative signals. Deterministic rules might include: claims above a certain dollar threshold are high-risk, claims involving multiple properties are high-risk, claims with missing documentation are high-risk. AI-assisted signals might include: the claim description contains language patterns associated with complex losses, the claim involves coverage types that are frequently disputed, the claim involves a loss location with a history of fraud. These signals are combined to produce a risk score or risk category.

Risk is a routing signal, not a final decision. A high-risk claim is routed to a specialized queue or escalated for manual investigation, but it is not automatically denied or approved. A low-risk claim is routed to a standard processing queue, but it is not automatically approved without review. Risk assessment informs the routing decision, but it does not replace human judgment. Adjusters review high-risk claims with greater scrutiny, and they may override the risk assessment if they determine that the claim does not actually present the risk that the system identified.

Explainability is essential for risk assessment. When a claim is assigned a risk level, the system must be able to explain why. Which rules triggered? Which AI signals contributed to the risk score? What specific characteristics of the claim led to the risk assessment? This explainability allows adjusters to understand the basis for the routing decision and to make informed decisions about how to proceed. It also allows compliance and audit teams to review risk assessments and confirm that they are based on legitimate business factors rather than on bias or discrimination.

---

## K1.7A – Claim Document (Legal Artifact)

A Claim Document is a legal artifact submitted as evidence to support a claim. Documents are not files—they are evidentiary records that may be used in disputes, litigation, or regulatory review. Once a document is uploaded to the system, it becomes part of the permanent claim record and is treated with the same immutability and traceability requirements as the claim itself.

Documents are immutable by design. Once uploaded, a document cannot be modified, replaced, or deleted. If a document needs to be corrected, a new document must be uploaded and the original document must be marked as superseded. This immutability ensures that the chain of custody is preserved and that the audit trail accurately reflects what documents were available at each stage of processing. Modifying or deleting documents would break the chain of custody and undermine the regulatory defensibility of the claim record.

The chain-of-custody concept is central to document handling. The system must be able to prove when a document was received, who provided it, and that it has not been altered since receipt. Every document upload is recorded in the audit log with timestamp, uploader identity, and document metadata. Every document access is recorded in the audit log with timestamp, accessor identity, and access intent. This comprehensive tracking ensures that the system can demonstrate the provenance and integrity of every document in the claim record.

Documents are stored separately from claim data. The document content (the bytes of the PDF, image, or other file format) is stored in blob storage, not in the database. The database stores only document metadata: document identifier, claim identifier, filename, document type, upload timestamp, uploader identity, and storage location reference. This separation ensures that the database remains performant and that large files do not bloat the transactional database. It also allows the system to apply different retention and lifecycle policies to document content versus document metadata.

Documents are legal artifacts, not temporary files. They are subject to regulatory retention requirements and may need to be preserved for years after the claim is closed. The system enforces retention policies through lifecycle management rules that transition documents from hot to cool to archive storage tiers over time. These transitions reduce storage costs while maintaining compliance with retention requirements. Documents are never deleted until the retention period has expired and regulatory requirements have been satisfied.

---

## K1.8 – Audit Event

An Audit Event is a record of a meaningful action taken by the system or by a user. Audit events include: claim submission, policy validation, document upload, AI extraction, human verification, risk assessment, claim routing, state transitions, and any modification to claim data. Each audit event records the action taken, the timestamp, the user who performed the action (if applicable), the result of the action, and any relevant context.

Audit logs must be append-only. Once an audit event is recorded, it cannot be modified, deleted, or overwritten. New events can be added to the log, but existing events are immutable. This append-only design ensures that the audit trail is a complete and accurate history of all actions taken on a claim. It prevents tampering with the audit trail and ensures that the history cannot be retroactively altered to hide mistakes or misconduct.

Audit logs are essential for regulatory defensibility. Regulators and auditors review audit logs to understand how claims were processed, what decisions were made, and what information was available at the time of processing. A complete and immutable audit trail demonstrates that the system operated according to established procedures and that decisions were made based on accurate information. If a claim is disputed or if a coverage decision is challenged, the audit trail provides evidence of how the decision was made and what information was considered.

Audit events must be recorded for every meaningful action, including actions that fail or produce errors. If a policy validation fails, that failure is recorded. If an AI extraction produces a low-confidence result, that low confidence is recorded. If a human verification corrects an AI extraction, both the original extraction and the correction are recorded. This comprehensive recording ensures that the audit trail tells the complete story of how a claim was processed, including any errors, corrections, or unusual circumstances.

---

## Domain Relationships

These domain concepts are interconnected and interdependent. A Claim is always tied to a Policy and a Loss Date. Coverage-in-Force validation determines whether the Policy was active on the Loss Date. The Policy Snapshot captures the policy details at the time of validation. AI extraction produces Unverified outputs that must be reviewed through Human Verification. Risk assessment produces a Risk signal that drives routing decisions. All actions are recorded as Audit Events in an append-only log. Claim Lifecycle States progress sequentially, with each transition recorded in the audit trail.

Understanding these relationships is essential for understanding how the system operates and how decisions are made. Each concept reinforces the others, creating a coherent framework that ensures accuracy, accountability, and regulatory defensibility.

---

## Related Documents

- **Product Contract**: System intent and boundaries
- **README.md**: Project overview and business context
- **ARCHITECTURE.md**: System design and technical architecture
- **REQUIREMENTS.md**: Detailed functional and non-functional requirements

---

**Document Owner**: Product Management  
**Last Updated**: February 2026  
**Next Review**: Q2 2026
