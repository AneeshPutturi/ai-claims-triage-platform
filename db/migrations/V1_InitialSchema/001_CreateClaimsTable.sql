-- =============================================
-- Migration: V1 - Create Claims Table (Draft)
-- Description: Creates the Claims table without constraints
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: The Claims table is the authoritative record for a claim's lifecycle.
-- It represents the system of record for every claim submitted, validated, 
-- verified, and triaged through this platform.
-- =============================================

-- Check if table already exists (idempotency)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Claims' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Claims
    (
        -- Primary identifier: System-generated unique identifier
        -- Immutable once assigned
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        
        -- Business identifier: Human-readable claim number
        -- Format: Organizational convention (e.g., 2026-000001)
        -- Immutable once assigned
        ClaimNumber NVARCHAR(50) NOT NULL,
        
        -- Policy reference: External policy identifier from claimant
        -- Used to look up coverage information
        -- Immutable once assigned
        PolicyNumber NVARCHAR(100) NOT NULL,
        
        -- Loss date: Date the loss occurred as reported by claimant
        -- Must be specific calendar date, not range or estimate
        -- Immutable once assigned
        LossDate DATE NOT NULL,
        
        -- Loss type: Type of loss (e.g., PropertyDamage, Liability, BusinessInterruption)
        -- Immutable once assigned
        LossType NVARCHAR(100) NOT NULL,
        
        -- Loss location: Where the loss occurred
        -- Immutable once assigned
        LossLocation NVARCHAR(500) NOT NULL,
        
        -- Loss description: Textual description from claimant
        -- May be updated during verification if adjuster clarifies
        LossDescription NVARCHAR(4000) NULL,
        
        -- Lifecycle state: Current state of claim
        -- Values: Submitted, Validated, Verified, Triaged
        -- Only column that changes as claim progresses
        Status NVARCHAR(50) NOT NULL,
        
        -- Creation timestamp: When claim was created in system
        -- Immutable - establishes legal trigger for insurer's duty to respond
        CreatedAt DATETIME2(7) NOT NULL,
        
        -- Update timestamp: When claim was last modified
        -- Updated automatically on any column change
        UpdatedAt DATETIME2(7) NOT NULL,
        
        -- Submitter identity: Who submitted the claim
        -- Immutable - establishes accountability
        SubmittedBy NVARCHAR(255) NOT NULL
    );
    
    PRINT 'Claims table created successfully';
END
ELSE
BEGIN
    PRINT 'Claims table already exists - skipping creation';
END
GO
