# Claims Intake Backend API
## Clean Architecture Implementation

**Phase**: 5 - Backend API Skeleton  
**Status**: Complete  
**Date**: February 2026

---

## Architecture

This solution follows Clean Architecture principles with strict dependency flow:

```
┌─────────────────────────────────────────────┐
│              ClaimsIntake.API               │
│         (Controllers, Middleware)           │
└──────────────────┬──────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────┐
│        ClaimsIntake.Infrastructure          │
│    (Repositories, Services, Persistence)    │
└──────────────────┬──────────────────────────┘
                   │ implements
┌──────────────────▼──────────────────────────┐
│         ClaimsIntake.Application            │
│   (Commands, Handlers, Interfaces)          │
└──────────────────┬──────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────┐
│           ClaimsIntake.Domain               │
│    (Entities, Value Objects, Enums)         │
│              NO DEPENDENCIES                │
└─────────────────────────────────────────────┘
```

---

## Project Structure

### ClaimsIntake.Domain
Pure domain logic with no external dependencies:
- **Entities**: `Claim`, `PolicySnapshot`, `AuditEvent`
- **Value Objects**: `ClaimNumber`, `PolicyId`, `LossDate`
- **Enums**: `ClaimStatus`, `CoverageStatus`, `VerificationStatus`, `RiskLevel`

### ClaimsIntake.Application
Use cases and interfaces:
- **Commands**: `SubmitClaimCommand`
- **Handlers**: `SubmitClaimCommandHandler`
- **Interfaces**: `IClaimRepository`, `IPolicySnapshotRepository`, `IAuditLogRepository`
- **Services**: `IAuditLogService`, `IPolicyValidationService`

### ClaimsIntake.Infrastructure
Persistence and external services:
- **Repositories**: Explicit SQL with Dapper, managed identity connections
- **Services**: `AuditLogService`, `PolicyValidationService` (stub)

### ClaimsIntake.API
HTTP endpoints and middleware:
- **Controllers**: `ClaimsController` (POST /claims, GET /claims/{id})
- **Middleware**: `CorrelationIdMiddleware`, `ErrorHandlingMiddleware`

---

## Key Design Decisions

### No ORM Magic
- Explicit SQL queries using Dapper
- Full control over database interactions
- No hidden N+1 queries or lazy loading surprises

### Managed Identity
- SQL connections use Azure AD authentication
- No passwords in connection strings
- Least-privilege access enforced

### Audit Trail
- Every state-changing operation emits audit event
- Append-only audit log
- Actor and action always present

### State Transitions
- Domain entity enforces valid transitions
- Illegal transitions rejected at domain level
- Reason logged for audit

### Error Handling
- Consistent error format across all endpoints
- No stack traces leaked to clients
- Correlation ID for request tracking

---

## Running Locally

### Prerequisites
- .NET 8.0 SDK
- Azure SQL Database (deployed via Bicep)
- Azure AD authentication configured

### Configuration
Update `appsettings.Development.json` with your SQL server name:
```json
{
  "ConnectionStrings": {
    "ClaimsDatabase": "Server=tcp:YOUR-SQL-SERVER.database.windows.net,1433;Database=sqldb-claims-dev;Authentication=Active Directory Default;"
  }
}
```

### Run
```bash
cd src/ClaimsIntake.API
dotnet run
```

### Test Endpoints
```bash
# Health check
curl http://localhost:5000/health

# Submit claim
curl -X POST http://localhost:5000/api/claims \
  -H "Content-Type: application/json" \
  -d '{
    "policyNumber": "POL-2025-12345",
    "lossDate": "2026-02-01",
    "lossType": "PropertyDamage",
    "lossLocation": "123 Main St, Seattle, WA",
    "lossDescription": "Water damage from burst pipe",
    "submittedBy": "claimant@example.com"
  }'

# Get claim
curl http://localhost:5000/api/claims/{claimId}
```

---

## End-to-End FNOL Flow

1. **POST /claims** with valid FNOL data
2. Command handler generates claim number
3. Domain entity created with validation
4. Policy validation service queries external system (stub)
5. Policy snapshot created
6. Claim and snapshot persisted atomically
7. Audit event emitted
8. Claim marked as Validated if coverage in force
9. **201 Created** returned with claim ID
10. **GET /claims/{id}** retrieves claim
11. Audit log contains complete history

---

## What's NOT Included (By Design)

- ❌ Document upload (Phase 6)
- ❌ AI extraction (Phase 6+)
- ❌ Risk assessment logic (Phase 7)
- ❌ Azure OpenAI integration (Phase 7)
- ❌ UI (Future)

---

## Phase 5 Completion Checklist

- ✅ K5.1 - Solution structure with Clean Architecture
- ✅ K5.2 - Pure domain entities
- ✅ K5.3 - Domain enums matching database
- ✅ K5.4 - Immutable value objects with validation
- ✅ K5.5 - Repository interfaces
- ✅ K5.6 - Application commands
- ✅ K5.7 - Command handlers
- ✅ K5.8 - Infrastructure persistence with explicit SQL
- ✅ K5.9 - Audit logging service
- ✅ K5.10 - Dependency injection wiring
- ✅ K5.11 - API project skeleton
- ✅ K5.12 - POST /claims endpoint
- ✅ K5.13 - Atomic persistence (claim + snapshot)
- ✅ K5.14 - Audit log emission
- ✅ K5.15 - GET /claims/{id} endpoint
- ✅ K5.16 - Status transition enforcement
- ✅ K5.17 - Error handling strategy
- ✅ K5.18 - Correlation ID middleware
- ✅ K5.19 - Authorization guards (placeholder)
- ✅ K5.20 - End-to-end validation ready

---

**If FNOL isn't bulletproof, nothing else matters.**

Phase 5 is complete. The system can start, accept traffic, talk to the database safely, and leave an audit trail—without AI.
