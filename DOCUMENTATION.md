# Documentation Index
## AI-Driven Commercial Claims Intake & Triage Platform

Complete documentation for the platform organized by topic.

---

## 📚 Getting Started

- **[README.md](README.md)** - Project overview, business context, and architecture
- **[SETUP.md](SETUP.md)** - How to run, test, and deploy the platform
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines (to be created)

---

## 🏗️ Architecture & Design

### Business & Domain
- **[Product Contract](docs/product-contract.md)** - System intent, boundaries, and success criteria
- **[Domain Model](docs/domain-model.md)** - Core business concepts and terminology
- **[Data Model](docs/data-model.md)** - Database schema and persistence strategy

### Infrastructure
- **[Azure Topology](docs/azure-topology.md)** - Cloud architecture and service selection
- **[Infrastructure README](infra/README.md)** - Bicep deployment guide
- **[Database Migration Strategy](docs/db-migration-strategy.md)** - Migration approach and versioning

---

## 🤖 AI Integration & Policies

- **[AI Extraction Policy](docs/ai-extraction-policy.md)** - What AI can extract, schema versioning, prompt management
- **[Verification Policy](docs/verification-policy.md)** - Human-in-the-loop requirements and accountability
- **[Risk Assessment Policy](docs/risk-assessment-policy.md)** - Risk evaluation rules and AI observations
- **[Document Ingestion Policy](docs/document-ingestion-policy.md)** - Document handling as legal artifacts
- **[Triage & Routing Policy](docs/triage-routing-policy.md)** - Claim routing and queue management

---

## 📖 Implementation Phases

### Phase Summaries
- **[Phase 5: Backend API](src/PHASE5_SUMMARY.md)** - Clean Architecture implementation
- **[Phase 6: Document Ingestion](src/PHASE6_SUMMARY.md)** - Secure document storage
- **[Phase 7: AI Extraction](src/PHASE7_SUMMARY.md)** - Azure OpenAI integration
- **[Phase 8: Human Verification](src/PHASE8_SUMMARY.md)** - HITL workflows
- **[Phase 9: Risk Assessment](src/PHASE9_SUMMARY.md)** - Verified data risk evaluation
- **[Phase 10: Triage & Routing](src/PHASE10_SUMMARY.md)** - Final phase - routing to queues

### Source Code Organization
- **[Source README](src/README.md)** - Code structure and conventions

---

## 🗄️ Database

### Migrations
- **[V1 Initial Schema](db/migrations/V1_InitialSchema/)** - Complete database schema
  - [Execute All Migrations](db/migrations/V1_InitialSchema/999_ExecuteAllV1Migrations.sql)
  - [Validation Walkthrough](db/migrations/V1_InitialSchema/VALIDATION_WALKTHROUGH.sql)
  - Individual migration scripts (001-016)

---

## 🔧 Infrastructure as Code

### Bicep Modules
- **[Main Template](infra/main.bicep)** - Orchestration template
- **[Resource Group](infra/modules/resource-group.bicep)**
- **[SQL Server](infra/modules/sql-server.bicep)**
- **[SQL Database](infra/modules/sql-database.bicep)**
- **[Storage Account](infra/modules/storage-account.bicep)**
- **[Key Vault](infra/modules/key-vault.bicep)**
- **[Container Apps Environment](infra/modules/container-apps-environment.bicep)**
- **[Container App](infra/modules/container-app.bicep)**
- **[Log Analytics](infra/modules/log-analytics.bicep)**

### Parameters
- [Development](infra/parameters/dev.parameters.json)
- [Staging](infra/parameters/staging.parameters.json)
- [Production](infra/parameters/prod.parameters.json)

### Security
- **[Security Validation Script](infra/validate-security.sh)** - Infrastructure security checks

---

## 🎯 AI Assets

### Schemas
- **[Claim Extraction Schema v1](ai/schemas/claim-extraction-v1.json)** - Canonical extraction schema

### Prompts
- **[System Prompt v1](ai/prompts/system-prompt-v1.txt)** - Non-negotiable AI rules
- **[User Prompt Template v1](ai/prompts/user-prompt-template-v1.txt)** - Deterministic prompt template

---

## 💻 Source Code

### Domain Layer
**Location**: `src/ClaimsIntake.Domain/`

#### Entities
- [Claim](src/ClaimsIntake.Domain/Entities/Claim.cs)
- [PolicySnapshot](src/ClaimsIntake.Domain/Entities/PolicySnapshot.cs)
- [ClaimDocument](src/ClaimsIntake.Domain/Entities/ClaimDocument.cs)
- [ExtractedField](src/ClaimsIntake.Domain/Entities/ExtractedField.cs)
- [VerificationRecord](src/ClaimsIntake.Domain/Entities/VerificationRecord.cs)
- [RiskAssessment](src/ClaimsIntake.Domain/Entities/RiskAssessment.cs)
- [TriageDecision](src/ClaimsIntake.Domain/Entities/TriageDecision.cs)
- [AuditEvent](src/ClaimsIntake.Domain/Entities/AuditEvent.cs)

#### Enums
- [ClaimStatus](src/ClaimsIntake.Domain/Enums/ClaimStatus.cs)
- [CoverageStatus](src/ClaimsIntake.Domain/Enums/CoverageStatus.cs)
- [VerificationStatus](src/ClaimsIntake.Domain/Enums/VerificationStatus.cs)
- [RiskLevel](src/ClaimsIntake.Domain/Enums/RiskLevel.cs)

#### Value Objects
- [ClaimNumber](src/ClaimsIntake.Domain/ValueObjects/ClaimNumber.cs)
- [PolicyId](src/ClaimsIntake.Domain/ValueObjects/PolicyId.cs)
- [LossDate](src/ClaimsIntake.Domain/ValueObjects/LossDate.cs)

### Application Layer
**Location**: `src/ClaimsIntake.Application/`

#### Commands
- [SubmitClaimCommand](src/ClaimsIntake.Application/Commands/SubmitClaimCommand.cs)
- [UploadClaimDocumentCommand](src/ClaimsIntake.Application/Commands/UploadClaimDocumentCommand.cs)
- [ExtractClaimDataCommand](src/ClaimsIntake.Application/Commands/ExtractClaimDataCommand.cs)
- [VerifyExtractedFieldCommand](src/ClaimsIntake.Application/Commands/VerifyExtractedFieldCommand.cs)
- [EvaluateRiskCommand](src/ClaimsIntake.Application/Commands/EvaluateRiskCommand.cs)
- [TriageClaimCommand](src/ClaimsIntake.Application/Commands/TriageClaimCommand.cs)

#### Handlers
- [SubmitClaimCommandHandler](src/ClaimsIntake.Application/Handlers/SubmitClaimCommandHandler.cs)
- [UploadClaimDocumentCommandHandler](src/ClaimsIntake.Application/Handlers/UploadClaimDocumentCommandHandler.cs)
- [ExtractClaimDataCommandHandler](src/ClaimsIntake.Application/Handlers/ExtractClaimDataCommandHandler.cs)
- [VerifyExtractedFieldCommandHandler](src/ClaimsIntake.Application/Handlers/VerifyExtractedFieldCommandHandler.cs)
- [EvaluateRiskCommandHandler](src/ClaimsIntake.Application/Handlers/EvaluateRiskCommandHandler.cs)
- [TriageClaimCommandHandler](src/ClaimsIntake.Application/Handlers/TriageClaimCommandHandler.cs)
- [OverrideTriageCommandHandler](src/ClaimsIntake.Application/Handlers/OverrideTriageCommandHandler.cs)

#### Interfaces
- [IClaimRepository](src/ClaimsIntake.Application/Interfaces/IClaimRepository.cs)
- [IPolicySnapshotRepository](src/ClaimsIntake.Application/Interfaces/IPolicySnapshotRepository.cs)
- [IClaimDocumentRepository](src/ClaimsIntake.Application/Interfaces/IClaimDocumentRepository.cs)
- [IExtractedFieldRepository](src/ClaimsIntake.Application/Interfaces/IExtractedFieldRepository.cs)
- [IVerificationRecordRepository](src/ClaimsIntake.Application/Interfaces/IVerificationRecordRepository.cs)
- [IRiskAssessmentRepository](src/ClaimsIntake.Application/Interfaces/IRiskAssessmentRepository.cs)
- [ITriageDecisionRepository](src/ClaimsIntake.Application/Interfaces/ITriageDecisionRepository.cs)
- [IAuditLogRepository](src/ClaimsIntake.Application/Interfaces/IAuditLogRepository.cs)

#### Services
- [IAuditLogService](src/ClaimsIntake.Application/Services/IAuditLogService.cs)
- [IBlobStorageService](src/ClaimsIntake.Application/Services/IBlobStorageService.cs)
- [IPolicyValidationService](src/ClaimsIntake.Application/Services/IPolicyValidationService.cs)
- [IOpenAIService](src/ClaimsIntake.Application/Services/IOpenAIService.cs)
- [IExtractionService](src/ClaimsIntake.Application/Services/IExtractionService.cs)
- [IVerificationGuardService](src/ClaimsIntake.Application/Services/IVerificationGuardService.cs)
- [IRiskEvaluationService](src/ClaimsIntake.Application/Services/IRiskEvaluationService.cs)

### Infrastructure Layer
**Location**: `src/ClaimsIntake.Infrastructure/`

#### Repositories
- [ClaimRepository](src/ClaimsIntake.Infrastructure/Persistence/ClaimRepository.cs)
- [PolicySnapshotRepository](src/ClaimsIntake.Infrastructure/Persistence/PolicySnapshotRepository.cs)
- [ClaimDocumentRepository](src/ClaimsIntake.Infrastructure/Persistence/ClaimDocumentRepository.cs)
- [ExtractedFieldRepository](src/ClaimsIntake.Infrastructure/Persistence/ExtractedFieldRepository.cs)
- [VerificationRecordRepository](src/ClaimsIntake.Infrastructure/Persistence/VerificationRecordRepository.cs)
- [RiskAssessmentRepository](src/ClaimsIntake.Infrastructure/Persistence/RiskAssessmentRepository.cs)
- [TriageDecisionRepository](src/ClaimsIntake.Infrastructure/Persistence/TriageDecisionRepository.cs)
- [AuditLogRepository](src/ClaimsIntake.Infrastructure/Persistence/AuditLogRepository.cs)

#### Services
- [AuditLogService](src/ClaimsIntake.Infrastructure/Services/AuditLogService.cs)
- [BlobStorageService](src/ClaimsIntake.Infrastructure/Services/BlobStorageService.cs)
- [PolicyValidationService](src/ClaimsIntake.Infrastructure/Services/PolicyValidationService.cs)
- [OpenAIService](src/ClaimsIntake.Infrastructure/Services/OpenAIService.cs)
- [ExtractionService](src/ClaimsIntake.Infrastructure/Services/ExtractionService.cs)
- [VerificationGuardService](src/ClaimsIntake.Infrastructure/Services/VerificationGuardService.cs)
- [RiskEvaluationService](src/ClaimsIntake.Infrastructure/Services/RiskEvaluationService.cs)

### API Layer
**Location**: `src/ClaimsIntake.API/`

- [Program.cs](src/ClaimsIntake.API/Program.cs) - Application startup and DI configuration
- [ClaimsController](src/ClaimsIntake.API/Controllers/ClaimsController.cs) - REST API endpoints
- [CorrelationIdMiddleware](src/ClaimsIntake.API/Middleware/CorrelationIdMiddleware.cs)
- [ErrorHandlingMiddleware](src/ClaimsIntake.API/Middleware/ErrorHandlingMiddleware.cs)

---

## 🔍 Quick Reference

### Key Concepts
- **FNOL**: First Notice of Loss - initial claim submission
- **HITL**: Human-in-the-Loop - mandatory human verification
- **Verified Data Only**: AI output must be verified before use
- **Immutable Snapshots**: Historical records never modified
- **Audit Trail**: Every action logged with actor and timestamp

### AI Usage Points
1. **Document Extraction** (Phase 7) - Extract structured data from documents
2. **Risk Observations** (Phase 9) - Qualitative risk analysis (advisory only)

### Data Flow
```
FNOL → Document Upload → AI Extraction → Human Verification → Risk Assessment → Triage Routing
```

### Key Principles
- AI assists, humans decide, system proves it
- Rules first, AI second, humans always accountable
- Risk is a signal, not a verdict
- Routing is operational, not legal

---

## 📞 Support

For questions or issues:
1. Review relevant documentation above
2. Check phase summaries for implementation details
3. Review policy documents for business rules
4. Consult SETUP.md for testing procedures

---

**Last Updated**: February 2026  
**Version**: 1.0.0  
**Status**: Complete
