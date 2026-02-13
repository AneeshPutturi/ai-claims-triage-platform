-- =============================================
-- Migration: V1 - Create VerificationRecords Table
-- Description: Track human verification actions with accountability
-- Author: System Architect
-- Date: February 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VerificationRecords' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.VerificationRecords
    (
        VerificationId UNIQUEIDENTIFIER NOT NULL,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        ExtractedFieldId UNIQUEIDENTIFIER NULL, -- Nullable if verification applies to entire claim
        VerifiedBy NVARCHAR(255) NOT NULL,
        VerifiedAt DATETIME2(7) NOT NULL,
        ActionTaken NVARCHAR(50) NOT NULL, -- Confirmed, Corrected, Rejected
        CorrectedValue NVARCHAR(4000) NULL,
        VerificationNotes NVARCHAR(4000) NULL
    );
    PRINT 'VerificationRecords table created';
END
GO
