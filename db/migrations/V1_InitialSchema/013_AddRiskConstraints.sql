-- =============================================
-- Migration: V1 - Add Risk Constraints
-- Description: Ensure risk data is explainable and immutable
-- Author: System Architect
-- Date: February 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_RiskAssessment')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT PK_RiskAssessment PRIMARY KEY CLUSTERED (RiskAssessmentId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RiskAssessment_Claims')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT FK_RiskAssessment_Claims 
        FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId);
END
GO

-- One RiskAssessment per Claim (can be relaxed if reassessment is needed)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_RiskAssessment_ClaimId')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT UQ_RiskAssessment_ClaimId UNIQUE NONCLUSTERED (ClaimId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_RiskAssessment_RiskLevel')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT CK_RiskAssessment_RiskLevel 
        CHECK (RiskLevel IN ('Low', 'Medium', 'High', 'Critical'));
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_RiskAssessment_OverallScore')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT CK_RiskAssessment_OverallScore 
        CHECK (OverallScore >= 0.00 AND OverallScore <= 100.00);
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RiskAssessment_RiskAssessmentId')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT DF_RiskAssessment_RiskAssessmentId DEFAULT NEWID() FOR RiskAssessmentId;
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RiskAssessment_CreatedAt')
BEGIN
    ALTER TABLE dbo.RiskAssessment
    ADD CONSTRAINT DF_RiskAssessment_CreatedAt DEFAULT SYSUTCDATETIME() FOR CreatedAt;
END
GO

-- No updates allowed after insert (application-level enforcement)
-- Risk assessments are snapshots and must remain immutable for audit trail

PRINT 'RiskAssessment constraints applied';
GO
