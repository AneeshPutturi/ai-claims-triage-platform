// =============================================
// API Entry Point
// Description: Minimal API setup with dependency injection
// Author: API Team
// Date: February 2026
// =============================================

using ClaimsIntake.Application.Handlers;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Infrastructure.Persistence;
using ClaimsIntake.Infrastructure.Services;
using ClaimsIntake.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("ClaimsDatabase")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDatabase' not found");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Register repositories (Scoped - one instance per request)
builder.Services.AddScoped<IClaimRepository>(sp => new ClaimRepository(connectionString));
builder.Services.AddScoped<IPolicySnapshotRepository>(sp => new PolicySnapshotRepository(connectionString));
builder.Services.AddScoped<IAuditLogRepository>(sp => new AuditLogRepository(connectionString));

// Register services (Scoped)
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IPolicyValidationService, PolicyValidationService>();

// Register handlers (Scoped)
builder.Services.AddScoped<SubmitClaimCommandHandler>();

// Build app
var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add authentication/authorization (placeholder for now)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
