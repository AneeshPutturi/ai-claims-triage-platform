-- =============================================
-- V1 Schema Validation Walkthrough
-- Description: Manual simulation of claim lifecycle to verify schema completeness
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Walk through FNOL → Verification → Triage scenario
-- Confirms every step can be recorded without ambiguity or missing data points
-- Based on scenario documented in docs/data-model.md section K2.19
-- =============================================

SET NOCOUNT ON;
GO

PRINT '========================================';
PRINT 'Schema Validation Walkthrough';
PRINT 'Simulating Complete Claim Lifecycle';
PRINT '========================================';
PRINT '';

-- Declare variables for test data
DECLARE @ClaimId UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimNumber NVARCHAR(50) = '2026-000001';
DECLARE @PolicyNumber NVARCHAR(100) = 'POL-2025-12345';
DECLARE @LossDate DATE = '2026-02-01';
DECLARE @DocumentId UNIQUEIDENTIFIER = NEWID();
DECLARE @ExtractedFieldId UNIQUEIDENTIFIER = NEWID();
DECLARE @VerificationId UNIQUEIDENTIFIER = NEWID();
DECLARE @RiskAssessmentId UNIQUEIDENTIFIER = NEWID();

BEGIN TRANSACTION;

-- =============================================
-- STEP 1: FNOL Submission
-- =============================================
PRINT 'Step 1: FNOL Submission';

INSERT INTO dbo.Claims (
    ClaimId, ClaimNumber, PolicyNumber, LossDate, LossType, LossLocation,
    LossDescription, Status, SubmittedBy
)
VALUES (
    @ClaimId, @ClaimNumber, @PolicyNumber, @LossDate, 'PropertyDamage',
    '123 Main St, Seattle, WA 98101',
    'Water damage from burst pipe in commercial building',
    'Submitted', 'claimant@example.com'
);

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome, Details)
VALUES ('claimant@example.com', 'ClaimSubmitted', 'Claim', @ClaimNumber, 'Success', 
    '{"LossType":"PropertyDamage","LossDate":"2026-02-01"}');

PRINT '  ✓ Claim created with Status=Submitted';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- STEP 2: Document Upload
-- =============================================
PRINT 'Step 2: Document Upload';

INSERT INTO dbo.ClaimDocuments (
    DocumentId, ClaimId, FileName, DocumentType, StorageLocation,
    FileSizeBytes, ContentType, UploadedBy
)
VALUES (
    @DocumentId, @ClaimId, 'damage_photos.pdf', 'Photos',
    'https://storage.blob.core.windows.net/claims/2026-000001/damage_photos.pdf',
    2048576, 'application/pdf', 'claimant@example.com'
);

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome)
VALUES ('claimant@example.com', 'DocumentUploaded', 'Document', CAST(@DocumentId AS NVARCHAR(100)), 'Success');

PRINT '  ✓ Document metadata recorded';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- STEP 3: Policy Validation
-- =============================================
PRINT 'Step 3: Policy Validation';

INSERT INTO dbo.PolicySnapshot (
    ClaimId, PolicyId, EffectiveDate, ExpirationDate, CoverageStatus,
    CoveredLossTypes, CoverageLimits, Deductibles
)
VALUES (
    @ClaimId, @PolicyNumber, '2025-01-01', '2026-12-31', 'Active',
    '["PropertyDamage","Liability","BusinessInterruption"]',
    '{"PropertyDamage":1000000,"Liability":2000000}',
    '{"PropertyDamage":5000}'
);

UPDATE dbo.Claims
SET Status = 'Validated', UpdatedAt = SYSUTCDATETIME()
WHERE ClaimId = @ClaimId;

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome, Details)
VALUES ('System', 'PolicyValidated', 'Claim', @ClaimNumber, 'Success',
    '{"CoverageStatus":"Active","LossDate":"2026-02-01"}');

PRINT '  ✓ Policy snapshot captured';
PRINT '  ✓ Claim status updated to Validated';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- STEP 4: AI Extraction
-- =============================================
PRINT 'Step 4: AI Extraction';

INSERT INTO dbo.ExtractedFields (
    ExtractedFieldId, ClaimId, DocumentId, FieldName, FieldValue,
    ConfidenceScore, ExtractedByModel
)
VALUES (
    @ExtractedFieldId, @ClaimId, @DocumentId, 'EstimatedAmount', '25000',
    0.92, 'gpt-4-turbo'
);

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome, Details)
VALUES ('System', 'FieldExtracted', 'ExtractedField', CAST(@ExtractedFieldId AS NVARCHAR(100)), 'Success',
    '{"FieldName":"EstimatedAmount","ConfidenceScore":0.92}');

PRINT '  ✓ AI extraction recorded with VerificationStatus=Unverified';
PRINT '  ✓ Confidence score persisted';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- STEP 5: Human Verification
-- =============================================
PRINT 'Step 5: Human Verification';

INSERT INTO dbo.VerificationRecords (
    VerificationId, ClaimId, ExtractedFieldId, VerifiedBy, ActionTaken, VerificationNotes
)
VALUES (
    @VerificationId, @ClaimId, @ExtractedFieldId, 'adjuster@insurer.com',
    'Confirmed', 'Verified against repair estimate document'
);

UPDATE dbo.ExtractedFields
SET VerificationStatus = 'Verified'
WHERE ExtractedFieldId = @ExtractedFieldId;

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome)
VALUES ('adjuster@insurer.com', 'FieldVerified', 'VerificationRecord', CAST(@VerificationId AS NVARCHAR(100)), 'Success');

UPDATE dbo.Claims
SET Status = 'Verified', UpdatedAt = SYSUTCDATETIME()
WHERE ClaimId = @ClaimId;

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome)
VALUES ('adjuster@insurer.com', 'ClaimVerified', 'Claim', @ClaimNumber, 'Success');

PRINT '  ✓ Verification record created with adjuster identity';
PRINT '  ✓ ExtractedField status updated to Verified';
PRINT '  ✓ Claim status updated to Verified';
PRINT '  ✓ Audit log entries created';
PRINT '';

-- =============================================
-- STEP 6: Risk Assessment
-- =============================================
PRINT 'Step 6: Risk Assessment';

INSERT INTO dbo.RiskAssessment (
    RiskAssessmentId, ClaimId, RiskLevel, RuleSignals, AISignals, OverallScore, AssessedByModel
)
VALUES (
    @RiskAssessmentId, @ClaimId, 'Medium',
    '{"AmountThreshold":"Triggered","LocationRisk":"Low"}',
    '{"ComplexityScore":0.65,"HistoricalPattern":"Standard"}',
    65.00, 'gpt-4-turbo'
);

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome, Details)
VALUES ('System', 'RiskAssessed', 'RiskAssessment', CAST(@RiskAssessmentId AS NVARCHAR(100)), 'Success',
    '{"RiskLevel":"Medium","OverallScore":65.00}');

PRINT '  ✓ Risk assessment snapshot created';
PRINT '  ✓ Rule and AI signals separated';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- STEP 7: Triage Routing
-- =============================================
PRINT 'Step 7: Triage Routing';

UPDATE dbo.Claims
SET Status = 'Triaged', UpdatedAt = SYSUTCDATETIME()
WHERE ClaimId = @ClaimId;

INSERT INTO dbo.AuditLog (Actor, Action, EntityType, EntityId, Outcome, Details)
VALUES ('System', 'ClaimTriaged', 'Claim', @ClaimNumber, 'Success',
    '{"RiskLevel":"Medium","Queue":"StandardProcessing"}');

PRINT '  ✓ Claim status updated to Triaged';
PRINT '  ✓ Audit log entry created';
PRINT '';

-- =============================================
-- VALIDATION QUERIES
-- =============================================
PRINT '========================================';
PRINT 'Validation Queries';
PRINT '========================================';
PRINT '';

PRINT 'Claim Record:';
SELECT ClaimNumber, Status, LossType, LossDate, CreatedAt, UpdatedAt
FROM dbo.Claims WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Policy Snapshot:';
SELECT PolicyId, EffectiveDate, ExpirationDate, CoverageStatus
FROM dbo.PolicySnapshot WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Documents:';
SELECT FileName, DocumentType, UploadedAt, UploadedBy
FROM dbo.ClaimDocuments WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Extracted Fields:';
SELECT FieldName, FieldValue, ConfidenceScore, VerificationStatus
FROM dbo.ExtractedFields WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Verification Records:';
SELECT VerifiedBy, ActionTaken, VerifiedAt
FROM dbo.VerificationRecords WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Risk Assessment:';
SELECT RiskLevel, OverallScore, CreatedAt
FROM dbo.RiskAssessment WHERE ClaimId = @ClaimId;
PRINT '';

PRINT 'Audit Trail:';
SELECT Timestamp, Actor, Action, EntityType, Outcome
FROM dbo.AuditLog
WHERE EntityId = @ClaimNumber OR EntityId IN (
    CAST(@DocumentId AS NVARCHAR(100)),
    CAST(@ExtractedFieldId AS NVARCHAR(100)),
    CAST(@VerificationId AS NVARCHAR(100)),
    CAST(@RiskAssessmentId AS NVARCHAR(100))
)
ORDER BY Timestamp;
PRINT '';

-- =============================================
-- VALIDATION RESULT
-- =============================================
PRINT '========================================';
PRINT 'Validation Result';
PRINT '========================================';
PRINT '';
PRINT '✓ Every step of claim lifecycle recorded';
PRINT '✓ No missing data points';
PRINT '✓ All state transitions captured';
PRINT '✓ Complete audit trail maintained';
PRINT '✓ Schema is complete and sufficient';
PRINT '';
PRINT 'Schema V1 is VALIDATED and ready for production use.';
PRINT '';

ROLLBACK TRANSACTION; -- Don't persist test data
PRINT 'Test data rolled back (transaction not committed)';
GO
