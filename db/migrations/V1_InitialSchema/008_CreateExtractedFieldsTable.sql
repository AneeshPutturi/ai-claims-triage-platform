-- =============================================
-- Migration: V1 - Create ExtractedFields Table
-- Description: Creates table for AI-extracted data separate from verified data
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: AI-extracted data must never overwrite original claim data.
-- Separation protects against AI hallucination risk.
-- All extracted data is unverified by default until human review.
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExtractedFields' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ExtractedFields
    (
        -- Primary identifier
        ExtractedFieldId UNIQUEIDENTIFIER NOT NULL,
        
        -- Foreign key to Claims
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        
        -- Foreign key to source document
        DocumentId UNIQUEIDENTIFIER NOT NULL,
        
        -- Name of extracted field
        -- Examples: EstimatedAmount, InjuryDescription, PropertyAddress
        FieldName NVARCHAR(200) NOT NULL,
        
        -- Value extracted by AI
        -- Stored as NVARCHAR for flexibility (can be parsed by application)
        FieldValue NVARCHAR(4000) NULL,
        
        -- AI confidence score (0.0 to 1.0)
        -- Informs adjuster about reliability
        ConfidenceScore DECIMAL(5,4) NOT NULL,
        
        -- Verification status
        -- Values: Unverified, Verified, Corrected, Rejected
        -- Unverified by default
        VerificationStatus NVARCHAR(50) NOT NULL,
        
        -- Extraction timestamp
        ExtractedAt DATETIME2(7) NOT NULL,
        
        -- AI model identifier
        -- Examples: gpt-4, gpt-4-turbo
        -- Supports model versioning and performance analysis
        ExtractedByModel NVARCHAR(100) NOT NULL
    );
    
    PRINT 'ExtractedFields table created successfully';
END
ELSE
BEGIN
    PRINT 'ExtractedFields table already exists - skipping creation';
END
GO
