-- =============================================
-- Migration: V1 - Create PolicySnapshot Table (Draft)
-- Description: Creates PolicySnapshot table for point-in-time policy data
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Capture policy coverage status as of loss date, not current date.
-- External policy systems may be updated, corrected, or replaced over time.
-- Snapshot ensures claim record is self-contained and historically accurate.
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PolicySnapshot' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PolicySnapshot
    (
        -- Primary identifier: System-generated unique identifier
        SnapshotId UNIQUEIDENTIFIER NOT NULL,
        
        -- Foreign key to Claims table
        -- One-to-one relationship: each claim has exactly one policy snapshot
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        
        -- External policy identifier from policy system
        PolicyId NVARCHAR(100) NOT NULL,
        
        -- Policy effective date: When policy became active
        -- Immutable - used to determine coverage-in-force
        EffectiveDate DATE NOT NULL,
        
        -- Policy expiration date: When policy expired or was replaced
        -- Immutable - used to determine coverage-in-force
        ExpirationDate DATE NOT NULL,
        
        -- Coverage status on loss date
        -- Values: Active, Expired, Cancelled, Suspended
        -- Immutable - derived at snapshot time, persisted for clarity
        CoverageStatus NVARCHAR(50) NOT NULL,
        
        -- Loss types covered by policy
        -- Stored as JSON array for flexibility
        -- Immutable
        CoveredLossTypes NVARCHAR(2000) NOT NULL,
        
        -- Coverage limits applicable on loss date
        -- Stored as JSON for structured data
        -- Immutable
        CoverageLimits NVARCHAR(4000) NULL,
        
        -- Deductibles applicable on loss date
        -- Stored as JSON for structured data
        -- Immutable
        Deductibles NVARCHAR(4000) NULL,
        
        -- Snapshot creation timestamp
        -- Immutable - establishes when policy info was captured
        SnapshotCreatedAt DATETIME2(7) NOT NULL
    );
    
    PRINT 'PolicySnapshot table created successfully';
END
ELSE
BEGIN
    PRINT 'PolicySnapshot table already exists - skipping creation';
END
GO
