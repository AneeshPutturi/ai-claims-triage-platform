# Phase 5 Implementation Summary
## Backend API Skeleton - Clean Architecture

**Status**: In Progress  
**Date**: February 2026

---

## Completed Tasks

### K5.1 - Solution Structure ✓
Created Clean Architecture solution with proper dependency flow:
- `ClaimsIntake.Domain` - Pure domain logic, no dependencies
- `ClaimsIntake.Application` - Use cases and interfaces
- `ClaimsIntake.Infrastructure` - Persistence and external services
- `ClaimsIntake.API` - HTTP endpoints and middleware

### K5.2 - Domain Entities ✓
Created pure domain entities:
- `Claim` - Core aggregate with state transition logic
- `PolicySnapshot` - Immutable point-in-time policy record
- `AuditEvent` - Immutable audit log entry

### K5.3 - Domain Enums ✓
Created enums matching database constraints:
- `ClaimStatus` - Submitted, Validated, Verified, Triaged
- `CoverageStatus` - Active, Expired, Cancelled, Suspended
- `VerificationStatus` - Unverified, Verified, Corrected, Rejected
- `RiskLevel` - Low, Medium, High, Critical

### K5.4 - Domain Value Objects ✓
Created immutable value objects with validation:
- `ClaimNumber` - Format: YYYY-NNNNNN
- `PolicyId` - External policy identifier
- `LossDate` - Validated date with business rules

---

## Remaining Tasks

### K5.5 - Repository Interfaces
Define in Application layer:
```csharp
public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid claimId);
    Task<Claim?> GetByClaimNumberAsync(ClaimNumber claimNumber);
    Task AddAsync(Claim claim);
    Task UpdateAsync(Claim claim);
}

public interface IPolicySnapshotRepository
{
    Task AddAsync(PolicySnapshot snapshot);
    Task<PolicySnapshot?> GetByClaimIdAsync(Guid claimId);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditEvent auditEvent);
}
```

### K5.6 - Application Commands
```csharp
public record SubmitClaimCommand(
    string PolicyNumber,
    DateTime LossDate,
    string LossType,
    string LossLocation,
    string LossDescription,
    string SubmittedBy);

public record ValidatePolicyCommand(Guid ClaimId);
```

### K5.7 - Command Handlers
```csharp
public class SubmitClaimCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicySnapshotRepository _snapshotRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    
    public async Task<Guid> HandleAsync(SubmitClaimCommand command)
    {
        // 1. Generate claim number
        // 2. Create claim entity
        // 3. Validate policy and create snapshot
        // 4. Persist claim and snapshot atomically
        // 5. Emit audit event
        // 6. Return claim ID
    }
}
```

### K5.8 - Infrastructure Persistence Layer
Use ADO.NET or Dapper for explicit SQL:
```csharp
public class ClaimRepository : IClaimRepository
{
    private readonly string _connectionString;
    
    public async Task AddAsync(Claim claim)
    {
        const string sql = @"
            INSERT INTO Claims (ClaimId, ClaimNumber, PolicyNumber, ...)
            VALUES (@ClaimId, @ClaimNumber, @PolicyNumber, ...)";
        
        // Execute with managed identity connection
    }
}
```

### K5.9 - Audit Logging Service
```csharp
public class AuditLogService
{
    private readonly IAuditLogRepository _repository;
    
    public async Task LogClaimSubmittedAsync(Guid claimId, string claimNumber, string actor)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "ClaimSubmitted",
            entityType: "Claim",
            entityId: claimNumber,
            outcome: "Success");
        
        await _repository.AddAsync(auditEvent);
    }
}
```

### K5.10 - Dependency Injection
Register in API startup:
```csharp
services.AddScoped<IClaimRepository, ClaimRepository>();
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<SubmitClaimCommandHandler>();
services.AddScoped<AuditLogService>();
```

### K5.11 - API Project Skeleton
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
```

### K5.12 - POST /claims Endpoint
```csharp
[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitClaim([FromBody] SubmitClaimRequest request)
    {
        // Validate input
        // Invoke command handler
        // Return 201 Created with claim ID
    }
}
```

### K5.13 - Atomic Persistence
Ensure claim and policy snapshot are persisted in single transaction:
```csharp
using var transaction = await connection.BeginTransactionAsync();
try
{
    await _claimRepository.AddAsync(claim);
    await _snapshotRepository.AddAsync(snapshot);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### K5.14 - Audit Log Emission
Every state-changing operation emits audit event:
```csharp
await _auditLogService.LogClaimSubmittedAsync(claimId, claimNumber, actor);
```

### K5.15 - GET /claims/{id} Endpoint
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetClaim(Guid id)
{
    var claim = await _claimRepository.GetByIdAsync(id);
    if (claim == null) return NotFound();
    return Ok(claim);
}
```

### K5.16 - Status Transition Enforcement
Domain entity enforces valid transitions:
```csharp
public void MarkAsValidated()
{
    if (Status != ClaimStatus.Submitted)
        throw new InvalidOperationException("Invalid state transition");
    
    Status = ClaimStatus.Validated;
}
```

### K5.17 - Error Handling Strategy
```csharp
public class ErrorResponse
{
    public string Error { get; set; }
    public string Message { get; set; }
    public string? CorrelationId { get; set; }
}

// Middleware catches exceptions and returns consistent format
```

### K5.18 - Correlation ID Middleware
```csharp
app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    await next();
});
```

### K5.19 - Authorization Guards
```csharp
[Authorize(Roles = "ClaimSubmitter")]
[HttpPost]
public async Task<IActionResult> SubmitClaim(...)
```

### K5.20 - End-to-End Validation
Test complete FNOL flow:
1. POST /claims with valid data
2. Verify claim persisted in database
3. Verify policy snapshot persisted
4. Verify audit log entry created
5. GET /claims/{id} returns claim
6. Verify no manual DB edits required

---

## Architecture Principles

### Clean Architecture
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure implements Application interfaces
- API depends on Application and Infrastructure

### No Shortcuts
- No EF magic - explicit SQL queries
- No Azure references in Domain
- No business logic in controllers
- Every write emits audit event

### Managed Identity
- SQL connection uses Azure AD authentication
- No connection strings with passwords
- Least-privilege access enforced

---

## Next Steps

1. Complete repository implementations (K5.8)
2. Implement command handlers (K5.7)
3. Wire up dependency injection (K5.10)
4. Create API endpoints (K5.12, K5.15)
5. Add middleware (K5.17, K5.18)
6. Validate end-to-end flow (K5.20)

---

**If FNOL isn't bulletproof, nothing else matters.**
