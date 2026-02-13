# AI Extraction Policy
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the policy for AI-assisted extraction of claim data from unstructured documents. It establishes strict boundaries on what AI can extract, how extraction is performed, and how extracted data is treated. The fundamental principle is: **AI output is data, not truth.**

---

## K7.1 – Extraction Scope

AI extraction is limited to descriptive facts that can be objectively verified against source documents. AI is explicitly prohibited from extracting financial determinations, liability assessments, or approval decisions.

### Allowed Fields

The following fields may be extracted by AI:

- **Loss Date** - The date the loss occurred (must be a specific date, not a range)
- **Loss Location** - The address or location where the loss occurred
- **Loss Type** - The category of loss (PropertyDamage, Liability, BusinessInterruption, etc.)
- **Loss Description** - A factual description of what happened
- **Property Address** - The address of the damaged property (if different from loss location)
- **Estimated Damage Amount** - A dollar amount mentioned in the document (not calculated by AI)
- **Contact Information** - Phone numbers, email addresses mentioned in the document
- **Involved Parties** - Names of individuals or entities mentioned in the document

### Explicitly Excluded Fields

The following fields are **never** extracted by AI:

- **Payout Amount** - AI does not determine how much should be paid
- **Liability Determination** - AI does not assess who is at fault
- **Coverage Decision** - AI does not determine whether the claim is covered
- **Approval Status** - AI does not approve or deny claims
- **Risk Score** - AI does not assign risk levels (risk assessment is a separate process)
- **Fraud Indicators** - AI does not flag claims as fraudulent
- **Settlement Recommendations** - AI does not recommend settlement amounts

### Rationale

AI extraction is limited to descriptive facts because:

1. **Verifiability**: Descriptive facts can be verified by comparing AI output to source documents
2. **No Authority**: AI has no legal or business authority to make financial or coverage decisions
3. **Human Accountability**: Decisions that affect claim outcomes must be made by humans
4. **Regulatory Compliance**: Regulators require human oversight of claim decisions

---

## K7.2 – Canonical Extraction JSON Schema

All AI extraction output must conform to a strict JSON schema. The schema defines the exact structure, types, and allowed values for extraction results.

### Schema Version: v1

The canonical schema is stored in `/ai/schemas/claim-extraction-v1.json` and defines:

- **Field Names**: Exact names for each extractable field
- **Data Types**: String, number, date, boolean, or enum
- **Allowed Values**: For enum fields, the complete list of valid values
- **Optionality**: All fields are optional (AI returns null for unknown values)
- **No Nesting**: No nested free-form objects (flat structure only)

### Schema Enforcement

Every AI response is validated against the schema before persistence. Invalid responses are rejected and logged. No attempt is made to "fix" malformed output.

---

## K7.3 – Schema Versioning

Schema changes require a new version. Schema versions are immutable once deployed to production.

### Version Identifier

Each schema includes a version identifier embedded in the schema file:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "version": "v1",
  "title": "Claim Extraction Schema",
  ...
}
```

### Backward Compatibility

New schema versions must be backward compatible with existing extraction results. Fields can be added, but existing fields cannot be removed or have their types changed.

If a breaking change is required, a new schema version is created and existing extraction results are migrated or marked as using the old schema version.

### Schema Change Process

1. Create new schema file: `/ai/schemas/claim-extraction-v2.json`
2. Update extraction service to use new schema
3. Update validation logic to support both v1 and v2
4. Deploy to production
5. Monitor extraction results for errors
6. Deprecate v1 after migration period

---

## K7.4 – System Prompt (Non-Negotiable Rules)

The system prompt instructs the AI model on how to perform extraction. It is non-negotiable and must be followed exactly.

### System Prompt Content

```
You are a document extraction assistant for an insurance claims processing system.

Your task is to extract structured data from claim documents and return ONLY valid JSON matching the provided schema.

RULES:
1. Return ONLY valid JSON. No prose, no explanation, no commentary.
2. If a field value is not present in the document, return null for that field.
3. Do not infer, calculate, or estimate values that are not explicitly stated in the document.
4. Do not extract financial determinations, liability assessments, or approval decisions.
5. Extract only factual information that can be verified against the source document.
6. If you are uncertain about a value, return null rather than guessing.
7. Do not include any text outside the JSON structure.

The JSON schema is provided in the user message. Your response must conform to this schema exactly.
```

### Rationale

The system prompt establishes strict boundaries:
- **No prose**: Prevents the model from adding explanations or commentary
- **Null for unknown**: Prevents the model from guessing or inferring values
- **No calculations**: Prevents the model from performing arithmetic or logic
- **Factual only**: Limits extraction to verifiable facts
- **Schema conformance**: Ensures output can be validated and parsed

---

## K7.5 – User Prompt Template

The user prompt provides the document content and extraction instructions. It is deterministic and contains no open-ended language.

### User Prompt Template

```
Extract structured data from the following claim document.

Return a JSON object conforming to this schema:
{schema}

Document content:
{document_content}

Remember: Return ONLY valid JSON. Use null for any field not present in the document.
```

### Template Variables

- `{schema}`: The JSON schema definition (injected at runtime)
- `{document_content}`: The text content of the document (extracted via OCR or PDF parsing)

### Rationale

The user prompt is deterministic because:
- **No examples**: Examples can bias the model toward specific patterns
- **No ambiguity**: Clear instructions reduce variability in output
- **Schema reference**: Explicit schema ensures the model knows the expected structure

---

## K7.6 – Prompt Versioning Strategy

Prompts are versioned and tracked in source control. Old prompts are never silently replaced.

### Version Tracking

Prompt versions are stored in `/ai/prompts/` with version identifiers:
- `system-prompt-v1.txt`
- `user-prompt-template-v1.txt`

### Prompt Version Storage

Every extraction result stores the prompt version used:
- `SystemPromptVersion`: e.g., "v1"
- `UserPromptVersion`: e.g., "v1"

This allows auditors to understand exactly what instructions were given to the AI model for any extraction.

### Prompt Change Process

1. Create new prompt file with incremented version
2. Update extraction service to use new prompt
3. Store new prompt version with extraction results
4. Monitor extraction quality
5. Deprecate old prompt after validation period

---

## Related Documents

- **Domain Model**: `/docs/domain-model.md`
- **Data Model**: `/docs/data-model.md`
- **Document Ingestion Policy**: `/docs/document-ingestion-policy.md`

---

**Document Owner**: AI/ML Team & Compliance  
**Last Updated**: February 2026  
**Next Review**: Q2 2026
