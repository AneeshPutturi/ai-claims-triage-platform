-- =============================================
-- Migration: V1 - Add Accountability Constraints to VerificationRecords
-- Description: Ensure verification actions are always attributable
-- Author: System Architect
-- Date: February 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_VerificationRecords')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT PK_VerificationRecords PRIMARY KEY CLUSTERED (VerificationId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_VerificationRecords_Claims')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT FK_VerificationRecords_Claims 
        FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_VerificationRecords_ExtractedFields')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT FK_VerificationRecords_ExtractedFields 
        FOREIGN KEY (ExtractedFieldId) REFERENCES dbo.ExtractedFields(ExtractedFieldId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_VerificationRecords_ActionTaken')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT CK_VerificationRecords_ActionTaken 
        CHECK (ActionTaken IN ('Confirmed', 'Corrected', 'Rejected'));
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_VerificationRecords_VerificationId')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT DF_VerificationRecords_VerificationId DEFAULT NEWID() FOR VerificationId;
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_VerificationRecords_VerifiedAt')
BEGIN
    ALTER TABLE dbo.VerificationRecords
    ADD CONSTRAINT DF_VerificationRecords_VerifiedAt DEFAULT SYSUTCDATETIME() FOR VerifiedAt;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VerificationRecords_ClaimId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_VerificationRecords_ClaimId ON dbo.VerificationRecords(ClaimId);
END
GO

PRINT 'VerificationRecords accountability constraints applied';
GO
