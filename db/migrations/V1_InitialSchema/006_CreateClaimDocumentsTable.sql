-- =============================================
-- Migration: V1 - Create ClaimDocuments Table
-- Description: Creates table for document metadata and chain of custody
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Track documents as legal artifacts with provenance.
-- File content stored in blob storage, not database.
-- Metadata provides chain of custody for regulatory compliance.
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClaimDocuments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ClaimDocuments
    (
        -- Primary identifier: System-generated unique identifier
        DocumentId UNIQUEIDENTIFIER NOT NULL,
        
        -- Foreign key to Claims table
        -- One-to-many relationship: claim can have multiple documents
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        
        -- Original filename as provided by uploader
        -- Immutable - preserves original name for audit
        FileName NVARCHAR(500) NOT NULL,
        
        -- Document type classification
        -- Values: PoliceReport, Photos, RepairEstimate, MedicalRecords, Other
        -- Immutable once assigned
        DocumentType NVARCHAR(100) NOT NULL,
        
        -- Reference to blob storage location
        -- Typically Azure Blob Storage URI
        -- Immutable
        StorageLocation NVARCHAR(1000) NOT NULL,
        
        -- File size in bytes
        -- Immutable - supports storage management
        FileSizeBytes BIGINT NOT NULL,
        
        -- MIME type of file
        -- Examples: application/pdf, image/jpeg, image/png
        -- Immutable
        ContentType NVARCHAR(200) NOT NULL,
        
        -- Upload timestamp
        -- Immutable - establishes chain of custody
        UploadedAt DATETIME2(7) NOT NULL,
        
        -- Uploader identity
        -- Immutable - establishes accountability
        UploadedBy NVARCHAR(255) NOT NULL,
        
        -- Document status for soft deletion or supersession
        -- Values: Active, Superseded, Deleted
        -- Allows marking documents as replaced without physical deletion
        DocumentStatus NVARCHAR(50) NOT NULL
    );
    
    PRINT 'ClaimDocuments table created successfully';
END
ELSE
BEGIN
    PRINT 'ClaimDocuments table already exists - skipping creation';
END
GO
