# Data Model & Schema
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the data model and schema design for the AI-Driven Commercial Claims Intake & Triage Platform. It establishes the foundation of truth for the system—the persistent records that capture claim lifecycle, policy coverage, document provenance, AI extraction, human verification, risk assessment, and audit history. This is not a technical implementation guide; it is a conceptual contract that defines what data must be persisted, why it must be persisted, and how it relates to other data.

If the database is weak, the whole product is trash. This document ensures the database is strong.

---

## PHASE 2 – DATABASE & DATA CONTRACTS

### K2.1 – The Claims Table (Concept)

The Claims table is the authoritative record for a claim's lifecycle. It represents the system of record for every claim submitted, validated, verified, and triaged through this platform. A claim is a time-bound legal record that initiates the insurer's obligation to investigate and respond to a policyholder's request for coverage. The Claims table persists the essential facts about each claim: its unique identity, its current lifecycle state, its relationship to a policy and loss event, and the timestamps that establish when actions occurred.

Claim state must be persisted, not inferred. The system cannot derive a claim's current state by examining related records or by replaying event history. The state must be explicit, stored as a column in the Claims table, and updated through controlled transitions. This explicit persistence ensures that the system can answer the question "What is the current state of this claim?" with a single query, without ambiguity, without computation, and without risk of inconsistency. The Claims table is the single source of truth for claim state, and all other tables reference it as the authoritative record.

---

### K2.2 – Claim Identity & Immutability

A claim is uniquely identified by two attributes: a system-generated ClaimId (a surrogate key, typically a GUID or auto-incrementing integer) and a business-meaningful ClaimNumber (a human-readable identifier that may follow organizational conventions such as year-sequence format). The ClaimId is the primary key used for internal referential integrity. The ClaimNumber is the external identifier used in communications, reports, and integrations with downstream systems.

Certain attributes of a claim must never change once the claim is created. The ClaimNumber, once assigned, is immutable. The creation timestamp (CreatedAt) is immutable. The policy reference and loss date are immutable. These attributes establish the identity and legal context of the claim, and changing them would fundamentally alter what the claim represents. Immutability ensures traceability across systems. If a claim is referenced in an external system, in an email, in a report, or in a legal document, that reference must remain valid indefinitely. If the ClaimNumber could change, references would break, and the audit trail would become unreliable.

Updates to a claim must be controlled. Only specific attributes can be updated, and only through specific workflows. The lifecycle state can be updated through controlled state transitions. Descriptive fields such as loss description or estimated amount may be updated during verification. But identity attributes and timestamps cannot be updated. This control prevents accidental or malicious corruption of the claim record and ensures that the audit trail accurately reflects the history of the claim.

---

### K2.3 – Claim Lifecycle Persistence

Claim lifecycle state must be stored explicitly in the Claims table, not derived dynamically from related records. The system cannot determine a claim's state by examining whether verification records exist or whether risk assessment has been completed. The state must be a column in the Claims table, updated through controlled transitions, and queryable without joins or computation.

Explicit persistence is essential for auditability. When a claim is audited months or years after processing, the audit trail must show exactly what state the claim was in at any point in time. If state were derived dynamically, the audit trail would depend on the continued existence and accuracy of related records. If a verification record were deleted or corrupted, the derived state would change, and the audit trail would become unreliable. Explicit persistence ensures that the state is captured at the moment of transition and remains accurate regardless of changes to related records.

Explicit persistence is also essential for replaying history during disputes. If a claim is disputed, the system must be able to show exactly when the claim transitioned from Submitted to Validated, from Validated to Verified, and from Verified to Triaged. These transitions must be recorded in the audit log, and the current state must be stored in the Claims table. This dual recording—state in the Claims table, transitions in the audit log—ensures that the system can answer both "What is the current state?" and "When did the claim reach this state?"


---

### K2.4 – Claims Table Columns

The Claims table contains the following columns, each serving a specific purpose in establishing claim identity, lifecycle state, and temporal context:

**ClaimId**: The primary key, a system-generated unique identifier. This is the surrogate key used for all internal referential integrity. It is immutable and never exposed to external systems. It exists to ensure that every claim has a stable, collision-free identifier that can be used in foreign key relationships without risk of conflict or reuse.

**ClaimNumber**: The business-meaningful identifier, human-readable and following organizational conventions. This is the external identifier used in communications, reports, and integrations. It is immutable once assigned. It exists to provide a stable, recognizable reference that can be used by adjusters, policyholders, and external systems.

**PolicyNumber**: The policy reference provided by the claimant at FNOL submission. This is the external policy identifier used to look up coverage information. It is immutable. It exists to establish the relationship between the claim and the policy, enabling coverage validation.

**LossDate**: The date the loss occurred, as reported by the claimant. This is immutable. It exists to establish the point-in-time for coverage validation. Coverage must be validated as of the loss date, not as of the submission date or current date.

**LossType**: The type of loss (e.g., property damage, liability, business interruption). This is immutable. It exists to determine which coverage provisions apply and to support risk assessment and routing.

**LossLocation**: The location where the loss occurred. This is immutable. It exists to determine jurisdiction, applicable regulations, and coverage applicability.

**LossDescription**: A textual description of the loss as provided by the claimant. This may be updated during verification if the adjuster clarifies or corrects the description. It exists to provide context for investigation and risk assessment.

**Status**: The current lifecycle state of the claim (Submitted, Validated, Verified, Triaged). This is the only column that changes as the claim progresses through the workflow. It exists to provide an explicit, queryable record of where the claim is in its lifecycle.

**CreatedAt**: The timestamp when the claim was created in the system. This is immutable. It exists to establish the legal trigger for the insurer's duty to respond and to provide a temporal anchor for all subsequent actions.

**UpdatedAt**: The timestamp when the claim was last updated. This is updated automatically whenever any column in the Claims table is modified. It exists to provide a quick indicator of recent activity and to support concurrency control.

**SubmittedBy**: The identity of the user or system that submitted the claim. This is immutable. It exists to establish accountability and to support audit requirements.

No AI-related fields are stored in the Claims table. AI extraction results, confidence scores, and verification status are stored in separate tables (ExtractedFields, VerificationRecords). The Claims table contains only the authoritative, verified claim data. This separation ensures that AI-generated outputs never contaminate the authoritative claim record.

---

### K2.5 – PolicySnapshot Table Purpose

Policy data must be snapshotted at claim submission time instead of queried dynamically from the external policy system later. This snapshot approach is essential for three reasons: point-in-time coverage validation, legal defensibility, and protection against external policy system drift.

Point-in-time coverage validation requires that the system determine whether coverage was in force on the loss date, not on the current date. External policy systems may reflect the current state of the policy, but they may not reliably provide historical state as of an arbitrary past date. The policy may have been updated, corrected, or replaced since the loss date. The snapshot captures the policy state as it was known at the time of claim submission, ensuring that coverage validation is based on the information available at that moment.

Legal defensibility requires that the system preserve the exact policy information that was used to make coverage determinations. If a claim is audited or disputed years later, the audit trail must show exactly what policy information was available and what coverage determination was made based on that information. If the system queried the external policy system dynamically, the audit trail would depend on the continued availability and accuracy of that system. If the external system were updated, corrected, or decommissioned, the audit trail would become unreliable. The snapshot ensures that the claim record is self-contained and does not depend on external systems.

External policy system drift is a real risk. Policy systems may be migrated, replaced, or corrected over time. Data quality issues may be discovered and fixed. Policy records may be updated to reflect retroactive changes or corrections. If the claims system queried the policy system dynamically, these changes would affect the coverage determination retroactively, potentially invalidating previous decisions. The snapshot protects against this drift by capturing the policy state at a specific moment and preserving it independently of the external system.

---

### K2.6 – PolicySnapshot Columns

The PolicySnapshot table contains the following columns, each required to prove coverage-in-force at the loss date:

**SnapshotId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**ClaimId**: The foreign key linking this snapshot to the claim. This establishes the one-to-one relationship between a claim and its policy snapshot. It is immutable.

**PolicyId**: The external policy identifier from the policy system. This is the policy number or policy ID used to look up the policy. It is immutable.

**EffectiveDate**: The date the policy became active. This is immutable. It exists to determine whether the policy was in force on the loss date. If the loss date is before the effective date, the policy was not in force.

**ExpirationDate**: The date the policy expired or was replaced. This is immutable. It exists to determine whether the policy was in force on the loss date. If the loss date is after the expiration date, the policy was not in force.

**CoverageStatus**: The status of the policy on the loss date (Active, Expired, Cancelled, Suspended). This is immutable. It exists to provide an explicit record of whether coverage was in force. This is a derived value based on the loss date, effective date, and expiration date, but it is persisted explicitly to avoid ambiguity.

**CoveredLossTypes**: A list or structured representation of the loss types covered by the policy. This is immutable. It exists to determine whether the specific loss type reported in the claim is covered by the policy.

**CoverageLimits**: The coverage limits applicable to the policy on the loss date. This is immutable. It exists to provide context for risk assessment and to support downstream processing.

**Deductibles**: The deductibles applicable to the policy on the loss date. This is immutable. It exists to provide context for downstream processing.

**SnapshotCreatedAt**: The timestamp when the snapshot was created. This is immutable. It exists to establish when the policy information was captured and to support audit requirements.

The linkage to LossDate is implicit through the ClaimId foreign key. The snapshot is created at the time of claim validation, and the loss date is stored in the Claims table. The snapshot represents the policy state as of the loss date, not as of the snapshot creation date. This distinction is critical for historical accuracy.

The snapshot is immutable. Once created, it is never updated. If the external policy system is corrected or updated, the snapshot remains unchanged. This immutability ensures that the claim record reflects the policy information that was available at the time of processing, not the policy information that is available today.

---

### K2.7 – ClaimDocuments Table Purpose

Documents are tracked separately from claims because documents are legal artifacts with their own lifecycle, provenance, and chain of custody requirements. A claim may have zero, one, or many documents. Each document must be tracked individually with metadata that establishes when it was uploaded, who uploaded it, what type of document it is, and where it is stored.

Metadata matters more than file content in the database. The actual file content (the bytes of the PDF, image, or other file format) is stored in blob storage, not in the database. The database stores only the metadata: the document identifier, the claim it belongs to, the filename, the upload timestamp, the uploader identity, the document type, and the storage location reference. This separation ensures that the database remains performant and that large files do not bloat the transactional database.

Chain of custody is a legal requirement for documents that may be used as evidence in disputes or litigation. The system must be able to prove when a document was received, who provided it, and that it has not been altered since receipt. The ClaimDocuments table provides this chain of custody by recording the upload timestamp, the uploader identity, and by treating documents as immutable once uploaded. Documents cannot be modified or deleted; they can only be superseded by new versions.


---

### K2.8 – ClaimDocuments Columns

The ClaimDocuments table contains the following columns, each required for document traceability and chain of custody:

**DocumentId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**ClaimId**: The foreign key linking this document to the claim. This establishes the relationship between the document and the claim. It is immutable. A document belongs to exactly one claim.

**FileName**: The original filename as provided by the uploader. This is immutable. It exists to provide a recognizable reference for the document and to preserve the original filename for audit purposes.

**DocumentType**: The type of document (e.g., police report, photos, repair estimate, medical records). This may be assigned by the uploader or inferred by the system. It is immutable once assigned. It exists to support document organization and to enable filtering and search.

**StorageLocation**: A reference to the blob storage location where the file content is stored. This is typically a URI or path. It is immutable. It exists to enable retrieval of the file content when needed.

**FileSizeBytes**: The size of the file in bytes. This is immutable. It exists to support storage management and to provide context for processing (e.g., large files may require different handling).

**ContentType**: The MIME type of the file (e.g., application/pdf, image/jpeg). This is immutable. It exists to support proper rendering and processing of the file.

**UploadedAt**: The timestamp when the document was uploaded. This is immutable. It exists to establish the chain of custody and to provide a temporal anchor for document-related actions.

**UploadedBy**: The identity of the user or system that uploaded the document. This is immutable. It exists to establish accountability and to support audit requirements.

Documents are immutable. Once uploaded, a document cannot be modified or deleted. If a document needs to be corrected or replaced, a new document is uploaded, and the original document remains in the system with a flag or status indicating it has been superseded. This immutability ensures that the chain of custody is preserved and that the audit trail accurately reflects the documents that were available at each stage of processing.


---

### K2.9 – ExtractedFields Table Purpose

AI-extracted data must never overwrite original claim data. This separation of concerns is fundamental to the system's design and is essential for managing AI hallucination risk. When the system invokes Azure OpenAI to extract structured data from unstructured documents, the results are stored in the ExtractedFields table, not in the Claims table. The extracted data is marked as unverified by default and remains separate from the authoritative claim record until a human adjuster reviews and verifies it.

AI hallucination risk is real. AI systems can generate plausible-sounding but factually incorrect information. An AI might extract a claim amount that does not appear in the source document, or might summarize a loss description in a way that misrepresents the actual loss, or might identify a coverage type that is not mentioned in the claim. These errors can be subtle and difficult to detect, but they can lead to incorrect coverage determinations, invalid claim routing, or downstream processing errors.

Separation of concerns ensures that AI-generated outputs are treated as suggestions, not as authoritative data. The ExtractedFields table stores the AI's interpretation of the documents, along with confidence scores and verification status. The Claims table stores only the verified, authoritative claim data. This separation makes it impossible for AI hallucinations to contaminate the authoritative claim record without human review.

Unverified by default is an explicit design principle. When an AI extraction is performed, the extracted fields are stored with VerificationStatus set to "Unverified". They remain in this state until a human adjuster reviews them and either confirms their accuracy (setting VerificationStatus to "Verified") or corrects them (creating a new record with the corrected value and setting VerificationStatus to "Verified"). Only verified data is used for downstream processing. Unverified data is visible to adjusters for review but is never used for automated decision-making or routing.

---

### K2.10 – ExtractedFields Columns

The ExtractedFields table contains the following columns, each required to store AI-extracted data with confidence and verification status:

**ExtractedFieldId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**ClaimId**: The foreign key linking this extracted field to the claim. This establishes the relationship between the extracted data and the claim. It is immutable.

**DocumentId**: The foreign key linking this extracted field to the source document. This establishes provenance—which document was the source of this extracted data. It is immutable.

**FieldName**: The name of the field that was extracted (e.g., "EstimatedAmount", "InjuryDescription", "PropertyAddress"). This is immutable. It exists to identify what piece of information was extracted.

**FieldValue**: The value extracted by the AI. This is stored as text or JSON to accommodate various data types. It is immutable. It exists to preserve the AI's interpretation of the document.

**ConfidenceScore**: A numeric score (typically 0.0 to 1.0) indicating the AI's confidence in the extraction. This is immutable. It exists to inform the adjuster about the reliability of the extraction and to support filtering or prioritization of review tasks.

**VerificationStatus**: The verification status of the extracted field (Unverified, Verified, Corrected, Rejected). This is the only column that changes after creation. It exists to track whether the extracted data has been reviewed by a human and whether it is trusted for downstream processing.

**ExtractedAt**: The timestamp when the extraction was performed. This is immutable. It exists to provide a temporal anchor for the extraction and to support audit requirements.

**ExtractedByModel**: The identifier of the AI model that performed the extraction (e.g., "gpt-4", "gpt-4-turbo"). This is immutable. It exists to support model versioning and to enable analysis of model performance over time.

Confidence is persisted because it provides valuable context for human review. High-confidence extractions may require less scrutiny than low-confidence extractions. Adjusters can prioritize review of low-confidence extractions or can configure workflows to automatically escalate low-confidence extractions for manual review. Persisting confidence also enables analysis of AI performance over time, allowing the system to identify patterns in extraction accuracy and to improve model selection or prompt engineering.

---

### K2.11 – VerificationRecords Table Purpose

Human verification actions must be tracked independently because verification is a legal responsibility, not just a workflow step. When an adjuster reviews AI-extracted data and confirms its accuracy, that action carries legal and regulatory significance. The adjuster is asserting that the data is accurate and can be relied upon for downstream processing. This assertion must be recorded with the adjuster's identity, the timestamp, and the specific action taken.

Accountability is essential. If a claim is disputed or audited, the system must be able to show who verified the data and when. If an error is discovered, the system must be able to trace the error back to the verification step and identify the adjuster who performed the verification. This accountability protects the business by ensuring that verification is taken seriously and that adjusters understand the responsibility they are assuming.

Legal responsibility means that verification is not a casual review. It is a formal assertion that the data is accurate and can be relied upon. The VerificationRecords table captures this assertion and preserves it as part of the permanent audit trail. If a claim results in litigation, the verification records may be used as evidence to show that the insurer exercised due diligence in validating claim data.

Adjuster identity must be captured explicitly. The system must record not just that verification occurred, but who performed the verification. This identity is typically a user ID or email address that can be traced back to a specific individual. This traceability ensures that accountability is personal and that the verification cannot be attributed to a generic system account or anonymous user.

---

### K2.12 – VerificationRecords Columns

The VerificationRecords table contains the following columns, each required to prove who verified what and when:

**VerificationId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**ClaimId**: The foreign key linking this verification record to the claim. This establishes the relationship between the verification action and the claim. It is immutable.

**ExtractedFieldId**: The foreign key linking this verification record to the extracted field that was verified. This establishes which specific piece of data was verified. It is immutable. This may be null if the verification applies to the entire claim rather than a specific extracted field.

**VerifiedBy**: The identity of the adjuster who performed the verification. This is typically a user ID or email address. It is immutable. It exists to establish personal accountability for the verification action.

**VerifiedAt**: The timestamp when the verification was performed. This is immutable. It exists to provide a temporal anchor for the verification and to support audit requirements.

**ActionTaken**: The action taken by the adjuster (Confirmed, Corrected, Rejected). This is immutable. It exists to record what the adjuster did during verification. "Confirmed" means the extracted data was accurate. "Corrected" means the adjuster modified the extracted data. "Rejected" means the adjuster determined the extracted data was incorrect and should not be used.

**CorrectedValue**: If the action was "Corrected", this field contains the corrected value. This is immutable. It exists to preserve the adjuster's correction and to support audit requirements.

**VerificationNotes**: Optional free-text notes provided by the adjuster explaining the verification decision. This is immutable. It exists to provide context for the verification action and to support audit requirements.

Corrections must be traceable. When an adjuster corrects an AI extraction, both the original extraction and the correction are preserved. The ExtractedFields table contains the original AI-generated value. The VerificationRecords table contains the corrected value. This dual recording ensures that the audit trail shows both what the AI extracted and what the adjuster determined to be accurate. This traceability is essential for understanding AI performance, for identifying patterns in extraction errors, and for defending verification decisions during audits.

---

### K2.13 – RiskAssessment Table Purpose

Risk assessment is a snapshot, not a live calculation. When the system performs risk assessment on a claim, the results are persisted in the RiskAssessment table. The risk level, the signals that contributed to the assessment, and the reasoning behind the assessment are all captured and stored. This snapshot approach ensures that the risk assessment is preserved as it was at the time of triage, not recalculated dynamically based on current data.

Explainability is essential for risk assessment. When a claim is assigned a risk level, the system must be able to explain why. Which business rules triggered? Which AI signals contributed to the risk score? What specific characteristics of the claim led to the risk assessment? This explainability allows adjusters to understand the basis for the routing decision and to make informed decisions about how to proceed. It also allows compliance and audit teams to review risk assessments and confirm that they are based on legitimate business factors rather than on bias or discrimination.

Replay during disputes requires that the risk assessment be preserved as it was at the time of triage. If a claim is disputed months or years later, the audit trail must show exactly what risk assessment was performed and what routing decision was made based on that assessment. If risk were calculated dynamically, the assessment might change over time as business rules are updated or as AI models are retrained. The snapshot ensures that the risk assessment is captured at the moment of triage and remains accurate regardless of changes to rules or models.

---

### K2.14 – RiskAssessment Columns

The RiskAssessment table contains the following columns, each required to persist both rule-based and AI-assisted risk signals:

**RiskAssessmentId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**ClaimId**: The foreign key linking this risk assessment to the claim. This establishes the relationship between the risk assessment and the claim. It is immutable.

**RiskLevel**: The overall risk level assigned to the claim (Low, Medium, High, Critical). This is immutable. It exists to provide a simple, actionable signal for routing decisions.

**RuleSignals**: A structured representation (typically JSON) of the business rules that triggered during risk assessment. This includes the rule identifiers, the rule descriptions, and the values that caused the rules to trigger. This is immutable. It exists to provide explainability for the rule-based component of risk assessment.

**AISignals**: A structured representation (typically JSON) of the AI-assisted signals that contributed to risk assessment. This includes the signal types, the confidence scores, and the reasoning provided by the AI. This is immutable. It exists to provide explainability for the AI-assisted component of risk assessment.

**OverallScore**: A numeric score (typically 0.0 to 1.0 or 0 to 100) representing the combined risk assessment. This is immutable. It exists to provide a quantitative measure of risk that can be used for sorting, filtering, or analytics.

**CreatedAt**: The timestamp when the risk assessment was performed. This is immutable. It exists to provide a temporal anchor for the assessment and to support audit requirements.

**AssessedByModel**: The identifier of the AI model that contributed to the risk assessment (if applicable). This is immutable. It exists to support model versioning and to enable analysis of model performance over time.

Separation of rule vs AI reasoning is explicit. The RuleSignals and AISignals columns are separate, allowing the system to distinguish between deterministic business rules and AI-assisted qualitative signals. This separation is important for explainability, for compliance, and for understanding the basis of risk assessments. Business rules are transparent and auditable. AI signals are probabilistic and require explanation. By separating them, the system makes it clear which component of the risk assessment is deterministic and which is AI-assisted.

---

### K2.15 – AuditLog Table Purpose

Audit logs must be append-only and untouchable. Once an audit event is recorded, it cannot be modified, deleted, or overwritten. New events can be added to the log, but existing events are immutable. This append-only design ensures that the audit trail is a complete and accurate history of all actions taken on a claim. It prevents tampering with the audit trail and ensures that the history cannot be retroactively altered to hide mistakes or misconduct.

Compliance and legal review depend on the integrity of the audit log. Regulators and auditors review audit logs to understand how claims were processed, what decisions were made, and what information was available at the time of processing. A complete and immutable audit trail demonstrates that the system operated according to established procedures and that decisions were made based on accurate information. If a claim is disputed or if a coverage decision is challenged, the audit trail provides evidence of how the decision was made and what information was considered.

Tamper resistance is achieved through append-only design and through technical controls such as write-once storage, cryptographic hashing, or blockchain-style chaining. The system must prevent unauthorized modification or deletion of audit records. Access to the audit log must be restricted to authorized personnel, and all access must itself be logged. These controls ensure that the audit trail is trustworthy and defensible.


---

### K2.16 – AuditLog Columns

The AuditLog table contains the following columns, each required to provide minimal but sufficient audit information:

**AuditId**: The primary key, a system-generated unique identifier. This is the surrogate key used for referential integrity. It is immutable.

**Timestamp**: The timestamp when the action occurred. This is immutable. It exists to provide a temporal anchor for the audit event and to enable chronological reconstruction of claim history.

**Actor**: The identity of the user or system that performed the action. This is typically a user ID, email address, or system identifier. It is immutable. It exists to establish accountability for the action.

**Action**: A short, standardized description of the action that was performed (e.g., "ClaimSubmitted", "PolicyValidated", "DocumentUploaded", "FieldVerified", "RiskAssessed", "ClaimTriaged"). This is immutable. It exists to provide a clear, queryable record of what happened.

**EntityType**: The type of entity that was affected by the action (e.g., "Claim", "Document", "ExtractedField", "VerificationRecord", "RiskAssessment"). This is immutable. It exists to provide context for the action and to enable filtering and search.

**EntityId**: The identifier of the specific entity that was affected by the action. This is typically the primary key of the affected record (e.g., ClaimId, DocumentId, ExtractedFieldId). It is immutable. It exists to link the audit event to the specific record that was affected.

**Outcome**: The outcome of the action (Success, Failure, PartialSuccess). This is immutable. It exists to record whether the action completed successfully or encountered errors.

**Details**: Optional structured data (typically JSON) providing additional context for the action. This is immutable. It exists to capture relevant details without bloating the audit log with excessive information.

Payloads should be minimal. The Details column should contain only the information necessary to understand the action and to support audit requirements. It should not contain full copies of claim records, documents, or other large data structures. Instead, it should contain only the changed fields, the key identifiers, and the relevant context. This minimalism ensures that the audit log remains performant and that storage costs are controlled.


---

### K2.17 – Referential Integrity Rules

Tables relate to each other through foreign key relationships that enforce referential integrity. These relationships prevent orphan records, ensure data consistency, and provide a clear structure for querying and navigation.

Claims is the aggregate root. All other tables reference Claims directly or indirectly. A claim is the central entity around which all other data is organized. PolicySnapshot, ClaimDocuments, ExtractedFields, VerificationRecords, RiskAssessment, and AuditLog all reference ClaimId as a foreign key. This design ensures that all data related to a claim can be retrieved by querying from the Claims table and following foreign key relationships.

Prevention of orphan records is enforced through foreign key constraints. A PolicySnapshot cannot exist without a corresponding Claim. A ClaimDocument cannot exist without a corresponding Claim. An ExtractedField cannot exist without a corresponding Claim and Document. A VerificationRecord cannot exist without a corresponding Claim and ExtractedField. A RiskAssessment cannot exist without a corresponding Claim. These constraints ensure that the database remains consistent and that queries do not encounter broken references.

Cascade behavior must be carefully controlled. When a claim is deleted (which should be rare and subject to strict controls), the system must decide whether to cascade the deletion to related records or to prevent deletion if related records exist. For this system, the recommended approach is to prevent deletion of claims that have related records. Claims are legal records and should not be deleted once they have progressed beyond initial submission. If a claim must be removed from the system, it should be marked as cancelled or voided rather than deleted, preserving the audit trail.

Foreign key relationships:
- PolicySnapshot.ClaimId → Claims.ClaimId (one-to-one)
- ClaimDocuments.ClaimId → Claims.ClaimId (one-to-many)
- ExtractedFields.ClaimId → Claims.ClaimId (one-to-many)
- ExtractedFields.DocumentId → ClaimDocuments.DocumentId (many-to-one)
- VerificationRecords.ClaimId → Claims.ClaimId (one-to-many)
- VerificationRecords.ExtractedFieldId → ExtractedFields.ExtractedFieldId (many-to-one, nullable)
- RiskAssessment.ClaimId → Claims.ClaimId (one-to-one or one-to-many if reassessment is supported)
- AuditLog.EntityId → various tables depending on EntityType (no enforced foreign key, but logical relationship)

---

### K2.18 – Concurrency & Versioning Strategy

Concurrent updates must be handled safely to prevent lost updates and data corruption. When two users or processes attempt to update the same claim simultaneously, the system must detect the conflict and prevent one update from silently overwriting the other.

RowVersion or equivalent is the recommended approach for optimistic concurrency control. Each table that supports updates includes a RowVersion column (or equivalent such as a timestamp or incrementing version number). When a record is read, the RowVersion is captured. When the record is updated, the update statement includes a WHERE clause that checks the RowVersion. If the RowVersion has changed since the record was read, the update fails, and the application is notified of the conflict.

Avoiding silent overwrites is critical for data integrity. Without concurrency control, the following scenario can occur: User A reads a claim with Status="Submitted". User B reads the same claim with Status="Submitted". User A updates the claim to Status="Validated". User B updates the claim to Status="Verified". User B's update overwrites User A's update, and the claim skips the Validated state. With RowVersion-based concurrency control, User B's update would fail because the RowVersion changed when User A updated the record. User B would be notified of the conflict and would need to re-read the claim and retry the update.

For most tables in this system, concurrency conflicts should be rare because records are largely immutable. Claims progress through a controlled workflow with explicit state transitions. Documents, PolicySnapshots, ExtractedFields, VerificationRecords, RiskAssessments, and AuditLog entries are all immutable once created. The primary concurrency concern is the Claims table, where the Status column may be updated by multiple processes or users. RowVersion-based concurrency control on the Claims table ensures that state transitions are serialized and that no transitions are lost.

---

### K2.19 – Validate Schema with a Claim Scenario

To validate that the schema can record every step of a claim lifecycle without ambiguity, we walk through a complete scenario from FNOL submission through triage:

**Step 1: FNOL Submission**
A claimant submits a First Notice of Loss through the intake channel. The system creates a record in the Claims table with ClaimId, ClaimNumber, PolicyNumber, LossDate, LossType, LossLocation, LossDescription, Status="Submitted", CreatedAt, UpdatedAt, and SubmittedBy. An AuditLog entry is created with Action="ClaimSubmitted", EntityType="Claim", EntityId=ClaimId, Actor=SubmittedBy, Timestamp=CreatedAt, Outcome="Success".

**Step 2: Document Upload**
The claimant uploads supporting documents (photos, police report, repair estimate). For each document, the system creates a record in the ClaimDocuments table with DocumentId, ClaimId, FileName, DocumentType, StorageLocation, FileSizeBytes, ContentType, UploadedAt, and UploadedBy. An AuditLog entry is created for each document with Action="DocumentUploaded", EntityType="Document", EntityId=DocumentId, Actor=UploadedBy, Timestamp=UploadedAt, Outcome="Success".

**Step 3: Policy Validation**
The system queries the external policy system to validate coverage-in-force. A record is created in the PolicySnapshot table with SnapshotId, ClaimId, PolicyId, EffectiveDate, ExpirationDate, CoverageStatus, CoveredLossTypes, CoverageLimits, Deductibles, and SnapshotCreatedAt. The system determines that coverage was in force on the loss date. The Claims table is updated with Status="Validated" and UpdatedAt=current timestamp. An AuditLog entry is created with Action="PolicyValidated", EntityType="Claim", EntityId=ClaimId, Actor="System", Timestamp=current timestamp, Outcome="Success", Details={"CoverageStatus":"Active"}.

**Step 4: AI Extraction**
The system invokes Azure OpenAI to extract structured data from the uploaded documents. For each extracted field, the system creates a record in the ExtractedFields table with ExtractedFieldId, ClaimId, DocumentId, FieldName, FieldValue, ConfidenceScore, VerificationStatus="Unverified", ExtractedAt, and ExtractedByModel. An AuditLog entry is created with Action="FieldExtracted", EntityType="ExtractedField", EntityId=ExtractedFieldId, Actor="System", Timestamp=ExtractedAt, Outcome="Success", Details={"FieldName":"EstimatedAmount","ConfidenceScore":0.92}.

**Step 5: Human Verification**
An adjuster reviews the extracted fields. For each field, the adjuster either confirms the extraction, corrects it, or rejects it. For each verification action, the system creates a record in the VerificationRecords table with VerificationId, ClaimId, ExtractedFieldId, VerifiedBy, VerifiedAt, ActionTaken, CorrectedValue (if applicable), and VerificationNotes. The corresponding ExtractedFields record is updated with VerificationStatus="Verified" or "Corrected" or "Rejected". An AuditLog entry is created with Action="FieldVerified", EntityType="VerificationRecord", EntityId=VerificationId, Actor=VerifiedBy, Timestamp=VerifiedAt, Outcome="Success", Details={"ActionTaken":"Confirmed"}.

After all fields are verified, the Claims table is updated with Status="Verified" and UpdatedAt=current timestamp. An AuditLog entry is created with Action="ClaimVerified", EntityType="Claim", EntityId=ClaimId, Actor=VerifiedBy, Timestamp=current timestamp, Outcome="Success".

**Step 6: Risk Assessment**
The system performs risk assessment using business rules and AI-assisted signals. A record is created in the RiskAssessment table with RiskAssessmentId, ClaimId, RiskLevel, RuleSignals, AISignals, OverallScore, CreatedAt, and AssessedByModel. An AuditLog entry is created with Action="RiskAssessed", EntityType="RiskAssessment", EntityId=RiskAssessmentId, Actor="System", Timestamp=CreatedAt, Outcome="Success", Details={"RiskLevel":"Medium","OverallScore":65}.

**Step 7: Triage Routing**
Based on the risk assessment, the system routes the claim to the appropriate queue. The Claims table is updated with Status="Triaged" and UpdatedAt=current timestamp. An AuditLog entry is created with Action="ClaimTriaged", EntityType="Claim", EntityId=ClaimId, Actor="System", Timestamp=current timestamp, Outcome="Success", Details={"RiskLevel":"Medium","Queue":"StandardProcessing"}.

**Validation Result**
Every step of the claim lifecycle has been recorded without ambiguity. The Claims table captures the current state. The PolicySnapshot table captures the coverage determination. The ClaimDocuments table captures the document provenance. The ExtractedFields table captures the AI-generated outputs. The VerificationRecords table captures the human verification actions. The RiskAssessment table captures the risk evaluation. The AuditLog table captures every meaningful action with timestamp, actor, and outcome. No data points are missing. The schema is complete and sufficient for the claim lifecycle.

---

### K2.20 – Lock the Data Model

The data model defined in this document is now locked. Schema changes require formal justification, impact analysis, and versioning. Casual refactoring is prohibited. The schema is marked as stable and serves as the foundation for all subsequent implementation work.

Any proposed schema change must be documented with:
- Justification: Why is the change necessary?
- Impact analysis: What tables, queries, and application code will be affected?
- Migration strategy: How will existing data be migrated to the new schema?
- Rollback plan: How will the change be reversed if it causes problems?

Schema versioning will be enforced through database migration scripts. Each schema change will be captured in a numbered migration script that can be applied to upgrade the database and rolled back to downgrade the database. This versioning ensures that schema changes are traceable, reproducible, and reversible.

The data model is the foundation of truth. If the database is weak, the whole product is trash. This data model is strong, defensible, and built for a regulated environment where errors are expensive and irreversible. It is ready for implementation.

---

## Summary of Tables

1. **Claims**: Authoritative record of claim lifecycle, identity, and state
2. **PolicySnapshot**: Point-in-time policy coverage record for legal defensibility
3. **ClaimDocuments**: Document metadata and chain of custody
4. **ExtractedFields**: AI-generated outputs with confidence and verification status
5. **VerificationRecords**: Human verification actions with accountability
6. **RiskAssessment**: Snapshot of risk evaluation with explainability
7. **AuditLog**: Append-only, immutable record of all meaningful actions

---

## Related Documents

- **Product Contract**: System intent and boundaries
- **Domain Model**: Core domain concepts and vocabulary
- **README.md**: Project overview and business context

---

**Document Owner**: Product Management & Data Architecture  
**Last Updated**: February 2026  
**Next Review**: Q2 2026  
**Status**: LOCKED - Schema changes require formal approval
