-- =============================================
-- Migration: V1 - Execute All Initial Schema Migrations
-- Description: Master script that applies all V1 migrations in correct order
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Single script to create entire initial schema.
-- Respects FK dependencies and applies migrations sequentially.
-- Idempotent - can be run multiple times safely.
-- =============================================

SET NOCOUNT ON;
GO

PRINT '========================================';
PRINT 'AI-Driven Commercial Claims Intake & Triage Platform';
PRINT 'Database Schema Migration V1';
PRINT 'Started: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- Execute migrations in dependency order
PRINT 'Step 1: Creating Claims table...';
:r 001_CreateClaimsTable.sql

PRINT 'Step 2: Adding constraints to Claims...';
:r 002_AddConstraintsToClaims.sql

PRINT 'Step 3: Adding concurrency control to Claims...';
:r 003_AddConcurrencyControlToClaims.sql

PRINT 'Step 4: Creating PolicySnapshot table...';
:r 004_CreatePolicySnapshotTable.sql

PRINT 'Step 5: Adding constraints to PolicySnapshot...';
:r 005_AddConstraintsToPolicySnapshot.sql

PRINT 'Step 6: Creating ClaimDocuments table...';
:r 006_CreateClaimDocumentsTable.sql

PRINT 'Step 7: Adding immutability constraints to ClaimDocuments...';
:r 007_AddImmutabilityConstraintsToClaimDocuments.sql

PRINT 'Step 8: Creating ExtractedFields table...';
:r 008_CreateExtractedFieldsTable.sql

PRINT 'Step 9: Adding constraints to ExtractedFields...';
:r 009_AddConstraintsToExtractedFields.sql

PRINT 'Step 10: Creating VerificationRecords table...';
:r 010_CreateVerificationRecordsTable.sql

PRINT 'Step 11: Adding accountability constraints to VerificationRecords...';
:r 011_AddAccountabilityConstraintsToVerificationRecords.sql

PRINT 'Step 12: Creating RiskAssessment table...';
:r 012_CreateRiskAssessmentTable.sql

PRINT 'Step 13: Adding risk constraints...';
:r 013_AddRiskConstraints.sql

PRINT 'Step 14: Creating AuditLog table...';
:r 014_CreateAuditLogTable.sql

PRINT 'Step 15: Enforcing append-only behavior on AuditLog...';
:r 015_EnforceAppendOnlyBehaviorOnAuditLog.sql

PRINT 'Step 16: Documenting foreign key relationships...';
:r 016_DefineForeignKeyRelationships.sql

PRINT '';
PRINT '========================================';
PRINT 'Migration V1 Complete';
PRINT 'Completed: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Review schema using SSMS or Azure Data Studio';
PRINT '2. Execute validation walkthrough (see docs/data-model.md K2.19)';
PRINT '3. Run application integration tests';
PRINT '4. Document any issues or observations';
PRINT '';
GO
