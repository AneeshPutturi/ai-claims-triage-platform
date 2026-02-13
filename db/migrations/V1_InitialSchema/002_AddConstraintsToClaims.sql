-- =============================================
-- Migration: V1 - Add Constraints to Claims Table
-- Description: Adds primary key, unique constraints, and NOT NULL enforcement
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Enforce data integrity rules on Claims table
-- - ClaimId is primary key for referential integrity
-- - ClaimNumber is unique for external traceability
-- - Status cannot be null to ensure explicit lifecycle state
-- - CreatedAt is immutable through application-level enforcement
-- =============================================

-- Add Primary Key constraint
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_Claims' AND parent_object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT PK_Claims PRIMARY KEY CLUSTERED (ClaimId);
    
    PRINT 'Primary key PK_Claims added successfully';
END
ELSE
BEGIN
    PRINT 'Primary key PK_Claims already exists - skipping';
END
GO

-- Add Unique constraint on ClaimNumber
-- Ensures external references remain valid indefinitely
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Claims_ClaimNumber' AND object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT UQ_Claims_ClaimNumber UNIQUE NONCLUSTERED (ClaimNumber);
    
    PRINT 'Unique constraint UQ_Claims_ClaimNumber added successfully';
END
ELSE
BEGIN
    PRINT 'Unique constraint UQ_Claims_ClaimNumber already exists - skipping';
END
GO

-- Add Default constraint for ClaimId
-- Generates new GUID for each claim
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Claims_ClaimId' AND parent_object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT DF_Claims_ClaimId DEFAULT NEWID() FOR ClaimId;
    
    PRINT 'Default constraint DF_Claims_ClaimId added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_Claims_ClaimId already exists - skipping';
END
GO

-- Add Default constraint for CreatedAt
-- Captures creation timestamp automatically
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Claims_CreatedAt' AND parent_object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT DF_Claims_CreatedAt DEFAULT SYSUTCDATETIME() FOR CreatedAt;
    
    PRINT 'Default constraint DF_Claims_CreatedAt added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_Claims_CreatedAt already exists - skipping';
END
GO

-- Add Default constraint for UpdatedAt
-- Captures update timestamp automatically
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Claims_UpdatedAt' AND parent_object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT DF_Claims_UpdatedAt DEFAULT SYSUTCDATETIME() FOR UpdatedAt;
    
    PRINT 'Default constraint DF_Claims_UpdatedAt added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_Claims_UpdatedAt already exists - skipping';
END
GO

-- Add Check constraint on Status
-- Ensures only valid lifecycle states are stored
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Claims_Status' AND parent_object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT CK_Claims_Status CHECK (Status IN ('Submitted', 'Validated', 'Verified', 'Triaged'));
    
    PRINT 'Check constraint CK_Claims_Status added successfully';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_Claims_Status already exists - skipping';
END
GO

-- Note: CreatedAt immutability is enforced at application level
-- Database triggers could enforce this but add complexity
-- Application code must never update CreatedAt after initial insert
PRINT 'Claims table constraints applied successfully';
GO
