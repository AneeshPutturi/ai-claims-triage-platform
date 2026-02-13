# V1 Initial Schema

**Version**: V1  
**Status**: LOCKED  
**Applied to Production**: TBD  
**Date Created**: February 2026

---

## Purpose

This migration creates the initial database schema for the AI-Driven Commercial Claims Intake & Triage Platform. It establishes the foundational tables for claims processing, policy validation, document management, AI extraction, human verification, risk assessment, and audit logging.

---

## Tables Created

1. **Claims** - Authoritative record of claim lifecycle
2. **PolicySnapshot** - Point-in-time policy coverage records
3. **ClaimDocuments** - Document metadata and chain of custody
4. **ExtractedFields** - AI-generated outputs with confidence scores
5. **VerificationRecords** - Human verification actions
6. **RiskAssessment** - Risk evaluation snapshots
7. **AuditLog** - Append-only audit trail

---

## Migration Scripts

Scripts must be executed in the following order:

1. `001_CreateClaimsTable.sql`
2. `002_AddConstraintsToClaims.sql`
3. `003_AddConcurrencyControlToClaims.sql`
4. `004_CreatePolicySnapshotTable.sql`
5. `005_AddConstraintsToPolicySnapshot.sql`
6. `006_CreateClaimDocumentsTable.sql`
7. `007_AddImmutabilityConstraintsToClaimDocuments.sql`
8. `008_CreateExtractedFieldsTable.sql`
9. `009_AddConstraintsToExtractedFields.sql`
10. `010_CreateVerificationRecordsTable.sql`
11. `011_AddAccountabilityConstraintsToVerificationRecords.sql`
12. `012_CreateRiskAssessmentTable.sql`
13. `013_AddRiskConstraints.sql`
14. `014_CreateAuditLogTable.sql`
15. `015_EnforceAppendOnlyBehaviorOnAuditLog.sql`
16. `016_DefineForeignKeyRelationships.sql`
17. `999_ExecuteAllV1Migrations.sql` (master script)

---

## LOCKED - NO EDITS PERMITTED

This migration version is LOCKED. No modifications are permitted to any scripts in this folder once V1 has been applied to any environment beyond local development.

If schema changes are required, create a new migration version (V2, V3, etc.) with forward-only changes.

---

## Validation

After applying this migration, validate the schema by executing:
```sql
sqlcmd -S <server> -d <database> -i VALIDATION_WALKTHROUGH.sql
```

Or execute the walkthrough scenario documented in `/docs/data-model.md` section K2.19.

---

## Schema Lock Declaration

**EFFECTIVE DATE**: February 2026  
**STATUS**: LOCKED

This schema version (V1) is now LOCKED and cannot be modified. Any changes to the database schema must be implemented as new migration versions (V2, V3, etc.) with forward-only changes.

### What "LOCKED" Means

1. **No Edits**: Migration scripts in this folder cannot be modified
2. **No Deletions**: Migration scripts cannot be removed
3. **Forward Only**: Schema changes require new migration versions
4. **Audit Trail**: Migration history must remain intact for regulatory compliance

### Why Schema Locking Matters

In regulated insurance systems, schema history is a legal requirement. If this migration has been applied to any environment beyond local development, modifying these scripts would:
- Break the audit trail
- Undermine regulatory defensibility
- Create inconsistency between source control and deployed schemas
- Violate compliance requirements

### If You Need to Change the Schema

Create a new migration version:
1. Create folder: `/db/migrations/V2_<description>/`
2. Write forward-only migration scripts
3. Document justification and impact
4. Follow the migration strategy in `/docs/db-migration-strategy.md`

---

**This lock is enforced through process discipline and code review.**  
**Pull requests that modify locked migrations will be rejected.**
