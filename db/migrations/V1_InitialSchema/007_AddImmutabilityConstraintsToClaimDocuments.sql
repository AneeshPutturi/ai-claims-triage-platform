-- =============================================
-- Migration: V1 - Add Immutability Constraints to ClaimDocuments
-- Description: Adds PK, FK, and documents immutability policy
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Enforce document immutability for chain of custody.
-- Documents are legal artifacts and cannot be modified after upload.
-- Updates restricted through application-level enforcement.
-- Physical deletion prevented - use DocumentStatus for soft deletion.
-- =============================================

-- Add Primary Key
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_ClaimDocuments' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT PK_ClaimDocuments PRIMARY KEY CLUSTERED (DocumentId);
    
    PRINT 'Primary key PK_ClaimDocuments added successfully';
END
ELSE
BEGIN
    PRINT 'Primary key PK_ClaimDocuments already exists - skipping';
END
GO

-- Add Foreign Key to Claims
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ClaimDocuments_Claims' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT FK_ClaimDocuments_Claims 
        FOREIGN KEY (ClaimId) 
        REFERENCES dbo.Claims(ClaimId)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION;
    
    PRINT 'Foreign key FK_ClaimDocuments_Claims added successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_ClaimDocuments_Claims already exists - skipping';
END
GO

-- Add Check constraint on DocumentStatus
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ClaimDocuments_DocumentStatus' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT CK_ClaimDocuments_DocumentStatus 
        CHECK (DocumentStatus IN ('Active', 'Superseded', 'Deleted'));
    
    PRINT 'Check constraint CK_ClaimDocuments_DocumentStatus added successfully';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_ClaimDocuments_DocumentStatus already exists - skipping';
END
GO

-- Add Default constraint for DocumentId
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ClaimDocuments_DocumentId' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT DF_ClaimDocuments_DocumentId DEFAULT NEWID() FOR DocumentId;
    
    PRINT 'Default constraint DF_ClaimDocuments_DocumentId added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_ClaimDocuments_DocumentId already exists - skipping';
END
GO

-- Add Default constraint for UploadedAt
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ClaimDocuments_UploadedAt' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT DF_ClaimDocuments_UploadedAt DEFAULT SYSUTCDATETIME() FOR UploadedAt;
    
    PRINT 'Default constraint DF_ClaimDocuments_UploadedAt added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_ClaimDocuments_UploadedAt already exists - skipping';
END
GO

-- Add Default constraint for DocumentStatus
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ClaimDocuments_DocumentStatus' AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT DF_ClaimDocuments_DocumentStatus DEFAULT 'Active' FOR DocumentStatus;
    
    PRINT 'Default constraint DF_ClaimDocuments_DocumentStatus added successfully';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_ClaimDocuments_DocumentStatus already exists - skipping';
END
GO

-- Create index on ClaimId for efficient queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClaimDocuments_ClaimId' AND object_id = OBJECT_ID('dbo.ClaimDocuments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ClaimDocuments_ClaimId
    ON dbo.ClaimDocuments(ClaimId);
    
    PRINT 'Index IX_ClaimDocuments_ClaimId created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_ClaimDocuments_ClaimId already exists - skipping';
END
GO

-- IMMUTABILITY POLICY (Application-Level Enforcement):
-- 1. Documents cannot be updated after insert (except DocumentStatus)
-- 2. Documents cannot be physically deleted (use DocumentStatus='Deleted')
-- 3. If document needs correction, upload new version and mark original as 'Superseded'
-- 4. Chain of custody preserved through immutable UploadedAt and UploadedBy
--
-- Database triggers could enforce this but add complexity.
-- Application code must respect immutability policy.
--
-- Rationale: Documents are legal artifacts that may be used as evidence.
-- Modifying or deleting documents breaks chain of custody and undermines
-- regulatory defensibility.

PRINT 'ClaimDocuments immutability constraints applied successfully';
GO
