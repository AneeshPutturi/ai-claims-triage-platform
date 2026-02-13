# Document Ingestion Policy
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## K6.2 – Allowed Document Types

The system accepts only the following document types:

- **PDF** (`.pdf`) - Portable Document Format
- **JPEG** (`.jpg`, `.jpeg`) - Joint Photographic Experts Group image format
- **PNG** (`.png`) - Portable Network Graphics image format

### Rationale for Restrictive List

The list of allowed document types is intentionally restrictive for security and processing predictability:

**Security**: PDF, JPEG, and PNG are well-understood formats with mature parsing libraries. They do not support executable code or macros that could introduce security vulnerabilities. Formats like Microsoft Office documents (`.docx`, `.xlsx`) or compressed archives (`.zip`, `.rar`) are explicitly rejected because they can contain embedded scripts, macros, or nested files that increase attack surface.

**Processing Predictability**: The allowed formats are widely supported by document processing and AI extraction tools. They have predictable structure and can be reliably parsed, rendered, and analyzed. Exotic or proprietary formats would require additional parsing logic, increase maintenance burden, and introduce unpredictable failure modes.

**Regulatory Compliance**: PDF is the de facto standard for legal and regulatory document exchange. JPEG and PNG are standard formats for photographic evidence. These formats are universally accepted by courts, regulators, and auditors.

### Explicit Rejection of Unsupported Formats

If a user attempts to upload a document with an unsupported file extension, the system rejects the upload immediately with a clear error message:

```
"Unsupported file type: .docx. Allowed types: PDF, JPEG, PNG."
```

The rejection occurs before any file content is uploaded to blob storage. This prevents unsupported files from consuming storage resources and ensures that only valid documents enter the system.

---

## K6.3 – Document Size Limits

The system enforces a maximum document size of **25 MB** per file.

### Rationale for Size Limit

**Operational Efficiency**: Large files consume significant bandwidth during upload and download. They increase processing time for AI extraction and document rendering. A 25 MB limit ensures that documents can be uploaded and processed within reasonable timeframes without degrading system performance.

**Cost Management**: Blob storage costs scale with data volume. AI extraction costs scale with document size (number of pages, image resolution). A size limit prevents a single large file from consuming disproportionate resources and inflating costs.

**User Experience**: Most claim documents (police reports, repair estimates, photos) are well under 25 MB. A 25 MB limit accommodates high-resolution photos and multi-page PDFs without imposing unreasonable restrictions on legitimate use cases.

**Security**: Extremely large files can be used in denial-of-service attacks to exhaust server memory or storage capacity. A size limit mitigates this risk.

### Rejection Behavior

If a user attempts to upload a document larger than 25 MB, the system rejects the upload immediately with a clear error message:

```
"Document size exceeds maximum allowed size of 25 MB. File size: 32 MB."
```

The rejection occurs before any file content is uploaded to blob storage. The user is instructed to reduce the file size (e.g., by compressing images or splitting multi-page PDFs) and retry the upload.

---

## K6.7 – Blob Naming Strategy

Documents are stored in Azure Blob Storage using a deterministic naming convention that prevents collisions and ensures traceability.

### Naming Convention

Blob names follow the format:

```
{ClaimId}/{DocumentId}.{extension}
```

Example:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/d9e8f7g6-h5i4-3210-jklm-no9876543210.pdf
```

### Rationale

**No User-Provided Names**: User-provided filenames are not used in blob names. User filenames may contain special characters, spaces, or non-ASCII characters that cause issues with blob storage APIs or downstream processing. User filenames are preserved in the `FileName` column of the `ClaimDocuments` table for display purposes, but they do not affect blob storage naming.

**Collision Prevention**: Using `DocumentId` (a GUID) as the blob name ensures that every document has a unique blob name. Even if two users upload files with the same filename to the same claim, the blobs will have different names because the `DocumentId` is unique.

**Claim Organization**: Grouping blobs by `ClaimId` creates a logical folder structure in blob storage. This makes it easy to list all documents for a specific claim, apply claim-level access policies, and manage lifecycle transitions at the claim level.

**Extension Preservation**: The file extension is preserved in the blob name to support content-type detection and to enable blob storage lifecycle policies that filter by file type.

---

## K6.19 – Failure Scenarios

Document upload is a multi-step process that involves blob storage upload, database metadata insertion, and audit logging. Failures can occur at any step, and the system must handle partial failures cleanly.

### Scenario 1: Blob Upload Fails

If the blob upload fails (e.g., due to network error, storage account unavailable, or insufficient permissions), the system does not insert metadata into the database. The upload request returns an error to the client, and no orphaned metadata is created.

**Behavior**: The client receives a 500 Internal Server Error with a message indicating that the document could not be uploaded. The client can retry the upload. No cleanup is required because no database records were created.

### Scenario 2: Database Insert Fails After Blob Upload

If the blob upload succeeds but the database insert fails (e.g., due to database unavailable, constraint violation, or transaction rollback), the blob remains in storage but no metadata exists in the database.

**Behavior**: The system logs the error and returns a 500 Internal Server Error to the client. The orphaned blob is not immediately deleted because deletion could fail and leave the system in an inconsistent state. Instead, a background cleanup job periodically scans for orphaned blobs (blobs that exist in storage but have no corresponding metadata record) and deletes them after a grace period (e.g., 24 hours).

### Scenario 3: Audit Log Insert Fails After Metadata Insert

If the blob upload and metadata insert succeed but the audit log insert fails, the document is successfully uploaded but the audit trail is incomplete.

**Behavior**: The system logs the error but does not roll back the metadata insert. The document is available for use, but the audit log does not contain a record of the upload. A background monitoring job detects missing audit entries and alerts operations teams to investigate.

### Scenario 4: Client Disconnects During Upload

If the client disconnects during the upload (e.g., due to network interruption or browser crash), the blob upload may be incomplete.

**Behavior**: Azure Blob Storage automatically cleans up incomplete uploads after a timeout period. No metadata is inserted into the database because the upload did not complete. The client can retry the upload.

---

## Related Documents

- **Domain Model**: `/docs/domain-model.md`
- **Data Model**: `/docs/data-model.md`
- **Azure Topology**: `/docs/azure-topology.md`

---

**Document Owner**: Product Management & Security  
**Last Updated**: February 2026  
**Next Review**: Q2 2026
