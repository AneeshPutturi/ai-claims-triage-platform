-- =============================================
-- Migration: V1 - Enforce Append-Only Behavior on AuditLog
-- Description: Prevent UPDATE and DELETE operations on AuditLog
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Audit logs must be tamper-resistant for regulatory compliance.
-- Once recorded, audit events cannot be modified or deleted.
-- Enforcement through database trigger.
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AuditLog')
BEGIN
    ALTER TABLE dbo.AuditLog
    ADD CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (AuditId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_AuditLog_Timestamp')
BEGIN
    ALTER TABLE dbo.AuditLog
    ADD CONSTRAINT DF_AuditLog_Timestamp DEFAULT SYSUTCDATETIME() FOR Timestamp;
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AuditLog_Outcome')
BEGIN
    ALTER TABLE dbo.AuditLog
    ADD CONSTRAINT CK_AuditLog_Outcome 
        CHECK (Outcome IN ('Success', 'Failure', 'PartialSuccess'));
END
GO

-- Create indexes for efficient querying
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_EntityType_EntityId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_EntityType_EntityId 
    ON dbo.AuditLog(EntityType, EntityId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Timestamp')
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp 
    ON dbo.AuditLog(Timestamp DESC);
END
GO

-- Create trigger to prevent UPDATE and DELETE
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AuditLog_PreventModification')
BEGIN
    EXEC('
    CREATE TRIGGER TR_AuditLog_PreventModification
    ON dbo.AuditLog
    AFTER UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        
        IF EXISTS (SELECT * FROM deleted)
        BEGIN
            RAISERROR (''AuditLog is append-only. UPDATE and DELETE operations are not permitted.'', 16, 1);
            ROLLBACK TRANSACTION;
        END
    END
    ');
    PRINT 'Trigger TR_AuditLog_PreventModification created';
END
ELSE
BEGIN
    PRINT 'Trigger TR_AuditLog_PreventModification already exists';
END
GO

-- WHY THIS EXISTS:
-- Audit logs are legal records that may be reviewed during regulatory audits,
-- compliance reviews, or litigation. Modifying or deleting audit records
-- undermines the integrity of the audit trail and exposes the organization
-- to regulatory penalties and legal liability.
--
-- The trigger prevents accidental or malicious modification of audit records.
-- Only INSERT operations are permitted.

PRINT 'AuditLog append-only behavior enforced';
GO
