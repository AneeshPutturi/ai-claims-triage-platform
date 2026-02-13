-- =============================================
-- Migration: V1 - Add Concurrency Control to Claims
-- Description: Adds RowVersion column for optimistic concurrency control
-- Author: System Architect
-- Date: February 2026
-- =============================================
-- Purpose: Protect against concurrent updates that could cause lost updates
-- or invalid state transitions.
--
-- Scenario without concurrency control:
-- - User A reads claim with Status='Submitted'
-- - User B reads same claim with Status='Submitted'
-- - User A updates to Status='Validated'
-- - User B updates to Status='Verified'
-- - Result: Claim skips Validated state (data corruption)
--
-- With RowVersion:
-- - User B's update fails because RowVersion changed
-- - User B must re-read claim and retry update
-- - Result: State transitions are serialized correctly
-- =============================================

-- Add RowVersion column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Claims') AND name = 'RowVersion')
BEGIN
    ALTER TABLE dbo.Claims
    ADD RowVersion ROWVERSION NOT NULL;
    
    PRINT 'RowVersion column added to Claims table';
    PRINT 'Optimistic concurrency control enabled';
    PRINT 'Application code must include RowVersion in WHERE clause for updates';
END
ELSE
BEGIN
    PRINT 'RowVersion column already exists - skipping';
END
GO

-- Create non-clustered index on RowVersion for efficient concurrency checks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Claims_RowVersion' AND object_id = OBJECT_ID('dbo.Claims'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Claims_RowVersion
    ON dbo.Claims(RowVersion);
    
    PRINT 'Index IX_Claims_RowVersion created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_Claims_RowVersion already exists - skipping';
END
GO

-- Usage pattern for application code:
-- 1. Read claim and capture RowVersion
-- 2. Perform business logic
-- 3. Update with WHERE clause including RowVersion
-- 4. If @@ROWCOUNT = 0, concurrency conflict detected - retry
--
-- Example:
-- UPDATE Claims 
-- SET Status = 'Validated', UpdatedAt = SYSUTCDATETIME()
-- WHERE ClaimId = @ClaimId AND RowVersion = @OriginalRowVersion
--
-- IF @@ROWCOUNT = 0
--     THROW 50001, 'Concurrency conflict detected', 1;

PRINT 'Concurrency control configured successfully';
GO
