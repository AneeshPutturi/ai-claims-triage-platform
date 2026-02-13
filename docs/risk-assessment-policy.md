# Risk Assessment Policy
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the policy for risk assessment of verified claim data. It establishes what risk means, how risk is evaluated, and how risk signals are used. The fundamental principle is: **Risk is a signal, not a verdict. Rules first, AI second, humans always accountable.**

Risk assessment is an operational prioritization mechanism. It helps route claims to the appropriate processing queue based on complexity, urgency, and potential issues. Risk assessment does not determine liability, does not approve or deny claims, and does not make financial decisions. Risk assessment informs humans, it does not replace them.

---

## K9.1 – What Risk Means (Business Definition)

Risk in this system represents the operational complexity and potential for processing issues associated with a claim. It is a routing signal that helps prioritize adjuster attention and determine which processing queue a claim should enter. Risk is not a fraud score. Risk is not a liability determination. Risk is not a payout recommendation.

High-risk claims require more scrutiny, more documentation, and more senior adjuster involvement. Low-risk claims can be processed through streamlined workflows with less oversight. Medium-risk claims fall in between. The risk level determines the processing path, not the outcome.

False positives are acceptable. If a low-risk claim is incorrectly flagged as high-risk, the worst outcome is that it receives more scrutiny than necessary. This wastes adjuster time but does not harm the claimant or expose the insurer to liability. False negatives are dangerous. If a high-risk claim is incorrectly flagged as low-risk, it may be processed through a streamlined workflow without adequate scrutiny, potentially leading to incorrect coverage determinations, missed fraud indicators, or regulatory violations.

The risk assessment philosophy is conservative. When in doubt, escalate. It is better to over-scrutinize than to under-scrutinize. Adjusters can always downgrade a claim's risk level after review, but they cannot undo the consequences of inadequate review.

---

## K9.2 – Risk Levels

Risk assessment assigns one of three levels to each claim:

**Low Risk**: The claim is straightforward, well-documented, and consistent. All required fields are present and verified. No contradictions or inconsistencies detected. Coverage is clearly in force. Loss type is standard and well-understood. The claim can be processed through a streamlined workflow with standard adjuster oversight.

Operational outcome: Route to standard processing queue. Assign to any available adjuster. No supervisor review required unless claim amount exceeds threshold.

**Medium Risk**: The claim has minor inconsistencies, missing optional fields, or qualitative concerns that warrant additional review. Coverage is in force but may have limitations or exclusions that need investigation. Loss type is standard but circumstances are unusual. The claim requires experienced adjuster review and may require additional documentation.

Operational outcome: Route to experienced adjuster queue. Supervisor review recommended. Additional documentation may be requested before processing.

**High Risk**: The claim has significant inconsistencies, contradictions, or red flags that require senior adjuster investigation. Coverage may be disputed or unclear. Loss type is unusual or high-severity. Critical fields are missing or contradictory. The claim requires senior adjuster review, supervisor approval, and potentially legal or compliance review before processing.

Operational outcome: Route to senior adjuster queue. Supervisor review mandatory. Legal or compliance review may be required. Additional investigation and documentation required before processing.

No financial thresholds are defined at this stage. Risk levels are based on data quality, consistency, and complexity, not on claim amount. Financial thresholds for routing and approval will be defined in future phases based on business policy.

---

## K9.3 – Deterministic Rule Categories

Risk assessment uses deterministic business rules that produce binary outcomes (pass/fail, present/absent, consistent/inconsistent). These rules are transparent, auditable, and explainable. No probabilistic language is used. Rules either trigger or they do not.

**Coverage Consistency Rules**: Verify that the verified extracted loss date falls within the policy effective and expiration dates from the policy snapshot. Verify that the verified extracted loss type is covered by the policy. Mismatch triggers risk escalation.

**Critical Field Completeness Rules**: Verify that all mandatory FNOL fields have been extracted and verified. Mandatory fields include: loss date, loss location, loss type, loss description. Missing verified fields trigger risk escalation.

**Data Inconsistency Rules**: Compare verified extracted fields against original FNOL input for contradictions. For example, if FNOL submission states loss date as January 15 but verified extracted field shows January 20, this is a contradiction. Contradictions trigger risk escalation and require adjuster investigation.

**Document Completeness Rules**: Verify that required supporting documents have been uploaded. For example, property damage claims require photos. Liability claims require incident reports. Missing required documents trigger risk escalation.

Rules are binary. They either trigger or they do not. There is no "maybe" or "probably". This binary nature ensures that rule outcomes are explainable and defensible. An adjuster can review the rule logic and understand exactly why a rule triggered.

---

## K9.4 – Rule Aggregation Logic

Individual rule outcomes are aggregated into a rule-based risk signal using deterministic logic. The aggregation logic is documented and transparent.

**Aggregation approach**:
- Each rule has a severity weight (Critical, Major, Minor)
- Critical rule triggers automatically escalate to High Risk
- Multiple Major rule triggers escalate to High Risk
- Multiple Minor rule triggers escalate to Medium Risk
- No rule triggers result in Low Risk (from rule perspective)

**Example rules and weights**:
- Coverage date mismatch: Critical
- Covered loss type mismatch: Critical
- Missing mandatory verified field: Major
- Data inconsistency between FNOL and extraction: Major
- Missing optional verified field: Minor
- Missing recommended document: Minor

The aggregation logic is deterministic and can be expressed as a decision tree or scoring matrix. This ensures that the same inputs always produce the same rule-based risk signal.

---

## K9.5 – AI Risk Signal Scope

AI is allowed to contribute qualitative observations that enrich context but do not determine risk level. AI analyzes verified text fields (loss description, verification notes, document content) to identify:

**Language ambiguity**: Vague or unclear descriptions that may require clarification. Example: "something happened to the building" is ambiguous compared to "fire damaged the roof".

**Unusual phrasing**: Language patterns that are inconsistent with typical claim narratives. Example: overly technical language in a consumer claim, or overly casual language in a commercial claim.

**Narrative red flags**: Qualitative concerns such as inconsistent timelines, missing causal explanations, or unusual claim circumstances. Example: "the damage was discovered three months after the incident" raises questions about causation.

**Completeness concerns**: Observations about missing context or details that would typically be present in a claim narrative. Example: property damage claim with no mention of how the damage was discovered.

AI is explicitly prohibited from:
- Assigning risk levels (Low, Medium, High)
- Making approval or denial recommendations
- Calculating fraud scores or fraud probabilities
- Determining liability or fault
- Recommending payout amounts
- Making coverage determinations

AI output is tagged as "Advisory" and is presented to adjusters as context, not as decisions. Adjusters are free to disregard AI observations if they determine them to be irrelevant or incorrect.

---

## K9.6 – AI Risk Prompt Design

The AI risk prompt is designed to elicit qualitative observations without allowing the model to make decisions. The prompt explicitly instructs the model to provide observations only, not recommendations.

**System Prompt**:
```
You are a claims review assistant. Your task is to analyze verified claim data and provide qualitative observations that may be relevant for adjuster review.

RULES:
1. Provide observations only, not recommendations or decisions.
2. Do not assign risk levels, fraud scores, or approval recommendations.
3. Focus on language clarity, narrative consistency, and completeness.
4. If you observe potential concerns, describe them factually without judgment.
5. Return observations in structured JSON format.
6. If no concerns are observed, return an empty observations array.
```

**User Prompt Template**:
```
Analyze the following verified claim data and provide qualitative observations.

Claim Data:
{verified_claim_data}

Return a JSON object with the following structure:
{
  "observations": [
    {
      "category": "language_ambiguity | unusual_phrasing | narrative_concern | completeness_concern",
      "description": "Factual description of the observation",
      "relevantField": "Field name where observation was made"
    }
  ]
}

Remember: Provide observations only. Do not make recommendations or assign risk levels.
```

The prompt is deterministic and constrains AI output to structured observations. The output schema is validated before persistence.

---

## K9.7 – Separation of Rule and AI Signals

Rule-based signals and AI-generated signals are stored separately and never merged into a single score. This separation ensures transparency and allows adjusters to understand the basis for risk assessment.

**RiskAssessment table structure**:
- `RuleSignals`: JSON containing deterministic rule outcomes (which rules triggered, why they triggered, severity weights)
- `AISignals`: JSON containing AI-generated observations (category, description, relevant field)
- `RiskLevel`: Final risk level determined by combining rule and AI signals
- `OverallScore`: Numeric score for sorting and analytics (optional)

The separation allows auditors to review rule logic independently of AI observations. It also allows the system to evolve rule logic or AI prompts without invalidating historical risk assessments.

---

## K9.8 – Risk Signal Combination Logic

The final risk level is determined by combining rule-based signals and AI-generated signals using explicit logic that prioritizes rules over AI.

**Combination logic**:
1. Calculate rule-based risk level using deterministic aggregation (K9.4)
2. Review AI observations for additional concerns
3. If rule-based risk is High, final risk is High (AI cannot downgrade)
4. If rule-based risk is Medium and AI flags critical concerns, escalate to High
5. If rule-based risk is Low and AI flags multiple concerns, escalate to Medium
6. If rule-based risk is Low and AI flags no concerns, final risk is Low

Rules always override AI. If deterministic rules indicate High Risk, AI observations cannot downgrade to Medium or Low. This ensures that objective, verifiable concerns are never overridden by subjective AI observations.

AI can only escalate risk, never downgrade. If AI identifies concerns that were not captured by rules, those concerns can escalate the risk level. But if AI identifies no concerns, it cannot override rule-based risk escalation.

The combination logic is documented in code and in this policy document. It is transparent, auditable, and explainable.

---

## K9.9 – Verified Data Requirement

Risk assessment can only be performed on verified data. Unverified AI-extracted fields are excluded from risk assessment. If any required field is unverified, risk assessment fails loudly with an error message identifying the unverified fields.

This requirement ensures that risk assessment is based on human-reviewed data, not on raw AI output. It reinforces the principle that AI output is data, not truth, and that human verification is required before data can be used for business decisions.

The verification guard service (Phase 8) is invoked at the beginning of risk assessment to validate that all required fields are verified. If the guard fails, risk assessment does not proceed.

---

## K9.10 – Risk Assessment Immutability

Risk assessments are immutable snapshots. Once a risk assessment is performed and persisted, it cannot be modified. If a claim's circumstances change (new documents uploaded, fields re-verified, additional information provided), a new risk assessment is performed and a new RiskAssessment record is created.

This immutability ensures that the audit trail accurately reflects the risk assessment that was performed at a specific point in time. It prevents retroactive changes that could obscure the basis for routing or processing decisions.

Multiple risk assessments for the same claim are allowed. The most recent assessment is used for routing decisions, but historical assessments are preserved for audit purposes.

---

## K9.11 – Explainability Requirement

Every risk assessment must be explainable. The RiskAssessment record includes:
- `RuleSignals`: Which rules triggered and why
- `AISignals`: What observations AI made and which fields they relate to
- `RiskLevel`: The final risk level
- `CreatedAt`: When the assessment was performed
- `AssessedByModel`: Which AI model was used (if applicable)

An adjuster reviewing a risk assessment can understand exactly why the claim was assigned a particular risk level. They can see which rules triggered, what AI observed, and how the signals were combined. This explainability is essential for trust, for training, and for regulatory compliance.

---

## K9.12 – Risk Assessment Audit Trail

Every risk assessment is logged in the AuditLog table with:
- **Action**: "RiskAssessed"
- **EntityType**: "RiskAssessment"
- **EntityId**: RiskAssessmentId
- **Actor**: "System" (risk assessment is automated, but based on verified human-reviewed data)
- **Timestamp**: When the assessment was performed
- **Outcome**: "Success" or "Failure"
- **Details**: JSON containing ClaimId, RiskLevel, rule trigger count, AI observation count

This audit trail provides a complete history of all risk assessments and can be queried to analyze risk assessment patterns, model performance, and adjuster workload distribution.

---

## K9.13 – Risk Philosophy

Risk assessment is a tool for humans, not a replacement for humans. Risk levels inform routing and prioritization decisions, but they do not determine claim outcomes. An adjuster can override a risk assessment if they determine it to be incorrect based on their professional judgment.

Risk assessment is conservative. When in doubt, escalate. It is better to over-scrutinize than to under-scrutinize. False positives are acceptable; false negatives are dangerous.

Risk assessment is transparent. Every risk level can be explained by reviewing the rule triggers and AI observations. There are no black-box scores or unexplainable algorithms.

Risk assessment is based on verified data. Unverified AI output is excluded. This ensures that risk assessment reflects human-reviewed information, not raw AI hallucinations.

Risk assessment is a signal, not a verdict. It informs humans, it does not replace them.

---

## Related Documents

- **Verification Policy**: `/docs/verification-policy.md`
- **AI Extraction Policy**: `/docs/ai-extraction-policy.md`
- **Data Model**: `/docs/data-model.md`
- **Domain Model**: `/docs/domain-model.md`

---

**Document Owner**: Compliance & Operations  
**Last Updated**: February 2026  
**Next Review**: Q2 2026  
**Status**: LOCKED - Changes require operational approval
