-- =============================================
-- Migration: V1 - Define Foreign Key Relationships Summary
-- Description: Documents all FK relationships across schema
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Claims is the aggregate root. All tables reference Claims
-- directly or indirectly. Foreign keys prevent orphan records and
-- ensure referential integrity.
-- =============================================

-- This script documents the foreign key relationships established
-- in previous migration scripts. No new constraints are added here.
-- This serves as a reference for understanding the schema structure.

PRINT '=== Foreign Key Relationships Summary ===';
PRINT '';
PRINT 'Claims (Aggregate Root)';
PRINT '  └─ PolicySnapshot.ClaimId → Claims.ClaimId (1:1)';
PRINT '  └─ ClaimDocuments.ClaimId → Claims.ClaimId (1:N)';
PRINT '  └─ ExtractedFields.ClaimId → Claims.ClaimId (1:N)';
PRINT '  └─ VerificationRecords.ClaimId → Claims.ClaimId (1:N)';
PRINT '  └─ RiskAssessment.ClaimId → Claims.ClaimId (1:1)';
PRINT '';
PRINT 'ClaimDocuments';
PRINT '  └─ ExtractedFields.DocumentId → ClaimDocuments.DocumentId (N:1)';
PRINT '';
PRINT 'ExtractedFields';
PRINT '  └─ VerificationRecords.ExtractedFieldId → ExtractedFields.ExtractedFieldId (N:1, nullable)';
PRINT '';
PRINT 'AuditLog';
PRINT '  └─ No enforced FKs (EntityId is logical reference only)';
PRINT '';
PRINT 'CASCADE BEHAVIOR:';
PRINT '  - All FKs use ON DELETE NO ACTION';
PRINT '  - Claims cannot be deleted if related records exist';
PRINT '  - Prevents accidental data loss';
PRINT '  - Claims are legal records and should not be deleted';
PRINT '';
PRINT 'ORPHAN PREVENTION:';
PRINT '  - PolicySnapshot cannot exist without Claim';
PRINT '  - ClaimDocuments cannot exist without Claim';
PRINT '  - ExtractedFields cannot exist without Claim and Document';
PRINT '  - VerificationRecords cannot exist without Claim';
PRINT '  - RiskAssessment cannot exist without Claim';
PRINT '';

-- Verify all foreign keys are in place
DECLARE @FKCount INT;
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE parent_object_id IN (
    OBJECT_ID('dbo.PolicySnapshot'),
    OBJECT_ID('dbo.ClaimDocuments'),
    OBJECT_ID('dbo.ExtractedFields'),
    OBJECT_ID('dbo.VerificationRecords'),
    OBJECT_ID('dbo.RiskAssessment')
);

PRINT 'Total Foreign Keys Defined: ' + CAST(@FKCount AS NVARCHAR(10));
PRINT '';
PRINT 'Expected: 7 foreign keys';
PRINT '  1. PolicySnapshot → Claims';
PRINT '  2. ClaimDocuments → Claims';
PRINT '  3. ExtractedFields → Claims';
PRINT '  4. ExtractedFields → ClaimDocuments';
PRINT '  5. VerificationRecords → Claims';
PRINT '  6. VerificationRecords → ExtractedFields';
PRINT '  7. RiskAssessment → Claims';
PRINT '';

IF @FKCount = 7
    PRINT '✓ All foreign keys are in place';
ELSE
    PRINT '⚠ Foreign key count mismatch - review previous migrations';

GO
