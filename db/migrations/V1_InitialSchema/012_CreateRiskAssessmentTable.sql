-- =============================================
-- Migration: V1 - Create RiskAssessment Table
-- Description: Persist risk decisions as snapshots for explainability
-- Author: System Architect
-- Date: February 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RiskAssessment' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.RiskAssessment
    (
        RiskAssessmentId UNIQUEIDENTIFIER NOT NULL,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        RiskLevel NVARCHAR(50) NOT NULL, -- Low, Medium, High, Critical
        RuleSignals NVARCHAR(MAX) NULL, -- JSON: business rules that triggered
        AISignals NVARCHAR(MAX) NULL, -- JSON: AI-assisted signals
        OverallScore DECIMAL(5,2) NOT NULL, -- 0.00 to 100.00
        CreatedAt DATETIME2(7) NOT NULL,
        AssessedByModel NVARCHAR(100) NULL -- AI model identifier if applicable
    );
    PRINT 'RiskAssessment table created';
END
GO
