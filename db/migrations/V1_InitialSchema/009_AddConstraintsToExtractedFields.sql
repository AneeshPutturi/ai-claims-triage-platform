-- =============================================
-- Migration: V1 - Add Constraints to ExtractedFields
-- Description: Enforce confidence score range and verification status values
-- Author: System Architect
-- Date: February 2026
-- =============================================

-- Add Primary Key
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_ExtractedFields' AND parent_object_id = OBJECT_ID('dbo.ExtractedFields'))
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT PK_ExtractedFields PRIMARY KEY CLUSTERED (ExtractedFieldId);
    
    PRINT 'Primary key PK_ExtractedFields added';
END
GO

-- Add Foreign Key to Claims
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ExtractedFields_Claims')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT FK_ExtractedFields_Claims 
        FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId);
    PRINT 'FK to Claims added';
END
GO

-- Add Foreign Key to ClaimDocuments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ExtractedFields_ClaimDocuments')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT FK_ExtractedFields_ClaimDocuments 
        FOREIGN KEY (DocumentId) REFERENCES dbo.ClaimDocuments(DocumentId);
    PRINT 'FK to ClaimDocuments added';
END
GO

-- Constrain ConfidenceScore between 0 and 1
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ExtractedFields_ConfidenceScore')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT CK_ExtractedFields_ConfidenceScore 
        CHECK (ConfidenceScore >= 0.0 AND ConfidenceScore <= 1.0);
    PRINT 'ConfidenceScore constraint added';
END
GO

-- Constrain VerificationStatus to enum values
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ExtractedFields_VerificationStatus')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT CK_ExtractedFields_VerificationStatus 
        CHECK (VerificationStatus IN ('Unverified', 'Verified', 'Corrected', 'Rejected'));
    PRINT 'VerificationStatus constraint added';
END
GO

-- Add defaults
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ExtractedFields_ExtractedFieldId')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT DF_ExtractedFields_ExtractedFieldId DEFAULT NEWID() FOR ExtractedFieldId;
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ExtractedFields_ExtractedAt')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT DF_ExtractedFields_ExtractedAt DEFAULT SYSUTCDATETIME() FOR ExtractedAt;
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_ExtractedFields_VerificationStatus')
BEGIN
    ALTER TABLE dbo.ExtractedFields
    ADD CONSTRAINT DF_ExtractedFields_VerificationStatus DEFAULT 'Unverified' FOR VerificationStatus;
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExtractedFields_ClaimId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExtractedFields_ClaimId ON dbo.ExtractedFields(ClaimId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExtractedFields_VerificationStatus')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExtractedFields_VerificationStatus ON dbo.ExtractedFields(VerificationStatus);
END
GO

PRINT 'ExtractedFields constraints applied successfully';
GO
