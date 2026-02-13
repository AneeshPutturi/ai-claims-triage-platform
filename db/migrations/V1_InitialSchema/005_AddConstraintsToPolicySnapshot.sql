-- =============================================
-- Migration: V1 - Add Constraints to PolicySnapshot Table
-- Description: Adds PK, FK, and date validity constraints
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Enforce referential integrity and business rules
-- - One PolicySnapshot per Claim (one-to-one relationship)
-- - EffectiveDate must be before ExpirationDate
-- - Foreign key to Claims ensures no orphan snapshots
-- =============================================

-- Add Primary Key
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_PolicySnapshot' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT PK_PolicySnapshot PRIMARY KEY CLUSTERED (SnapshotId);
    
    PRINT 'Primary key PK_PolicySnapshot added successfully';
END
ELSE
BEGIN
    PRINT 'Primary key PK_PolicySnapshot already exists - skipping';
END
GO

-- Add Foreign Key to Claims
-- Ensures every snapshot belongs to a valid claim
-- ON DELETE NO ACTION prevents orphan snapshots
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PolicySnapshot_Claims' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT FK_PolicySnapshot_Claims 
        FOREIGN KEY (ClaimId) 
        REFERENCES dbo.Claims(ClaimId)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION;
    
    PRINT 'Foreign key FK_PolicySnapshot_Claims added successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_PolicySnapshot_Claims already exists - skipping';
END
GO

-- Add Unique constraint on ClaimId
-- Enforces one-to-one relationship: one snapshot per claim
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_PolicySnapshot_ClaimId' AND object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT UQ_PolicySnapshot_ClaimId UNIQUE NONCLUSTERED (ClaimId);
    
    PRINT 'Unique constraint UQ_PolicySnapshot_ClaimId added successfully';
END
ELSE
BEGIN
    PRINT 'Unique constraint UQ_PolicySnapshot_ClaimId already exists - skipping';
END
GO

-- Add Check constraint: EffectiveDate < ExpirationDate
-- Ensures date range is logically valid
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_PolicySnapshot_DateRange' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT CK_PolicySnapshot_DateRange CHECK (EffectiveDate < ExpirationDate);
    
    PRINT 'Check constraint CK_PolicySnapshot_DateRange added successfully';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_PolicySnapshot_DateRange already exists - skipping';
END
GO

-- Add Check constraint on CoverageStatus
-- Ensures only valid status values are stored
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_PolicySnapshot_CoverageStatus' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT CK_PolicySnapshot_CoverageStatus 
        CHECK (CoverageStatus IN ('Active', 'Expired', 'Cancelled', 'Suspended'));
    
    PRINT 'Check constraint CK_PolicySnapshot_CoverageStatus added successfully';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_PolicySnapshot_CoverageStatus already exists - skipping';
END
GO

-- Add Default constraint for SnapshotId
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_PolicySnapshot_SnapshotId' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT DF_PolicySnapshot_SnapshotId DEFAULT NEWID() FOR SnapshotId;
    
    PRINT 'Default constraint DF_PolicySnapshot_SnapshotId added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_PolicySnapshot_SnapshotId already exists - skipping';
END
GO

-- Add Default constraint for SnapshotCreatedAt
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_PolicySnapshot_SnapshotCreatedAt' AND parent_object_id = OBJECT_ID('dbo.PolicySnapshot'))
BEGIN
    ALTER TABLE dbo.PolicySnapshot
    ADD CONSTRAINT DF_PolicySnapshot_SnapshotCreatedAt DEFAULT SYSUTCDATETIME() FOR SnapshotCreatedAt;
    
    PRINT 'Default constraint DF_PolicySnapshot_SnapshotCreatedAt added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_PolicySnapshot_SnapshotCreatedAt already exists - skipping';
END
GO

PRINT 'PolicySnapshot table constraints applied successfully';
GO
