-- =============================================
-- Migration: V1 - Create AuditLog Table
-- Description: Append-only audit logging for regulatory compliance
-- Author: System Architect
-- Date: February 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditId BIGINT IDENTITY(1,1) NOT NULL, -- Sequential for ordering
        Timestamp DATETIME2(7) NOT NULL,
        Actor NVARCHAR(255) NOT NULL, -- User ID or system identifier
        Action NVARCHAR(200) NOT NULL, -- ClaimSubmitted, PolicyValidated, etc.
        EntityType NVARCHAR(100) NOT NULL, -- Claim, Document, ExtractedField, etc.
        EntityId NVARCHAR(100) NOT NULL, -- ID of affected entity
        Outcome NVARCHAR(50) NOT NULL, -- Success, Failure, PartialSuccess
        Details NVARCHAR(4000) NULL -- JSON with minimal context
    );
    PRINT 'AuditLog table created';
END
GO
