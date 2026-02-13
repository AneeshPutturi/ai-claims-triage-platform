# Azure Topology & Architecture
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: Active

---

## Executive Summary

This document defines the high-level Azure architecture for the AI-Driven Commercial Claims Intake & Triage Platform. It describes which Azure services are used, why they exist, and how they relate to each other. The architecture is designed for enterprise-grade security, compliance, and operational excellence in a regulated insurance environment.

---

## Design Principles

The Azure topology is guided by the following principles:

**Infrastructure as Code**: All resources are provisioned via Bicep. No manual portal clicks. Every resource is reproducible, versioned, and auditable.

**Managed Identity Everywhere**: Services authenticate to each other using Azure Managed Identity, not connection strings or passwords. This eliminates credential management and reduces security risk.

**Separation of Concerns**: Each Azure service has a single, well-defined purpose. Storage is separate from compute. Secrets are separate from configuration. Data is separate from processing.

**Defense in Depth**: Security is enforced at multiple layers. Network isolation, identity-based access control, encryption at rest and in transit, and audit logging are all enabled by default.

**Cost Optimization**: Resources are sized appropriately for the workload. Lifecycle policies move data to cheaper storage tiers over time. Autoscaling ensures resources are used efficiently.

---

## Azure Services & Purpose

### Resource Group

The Resource Group is the logical container for all Azure resources in this platform. It provides a boundary for access control, cost tracking, and lifecycle management.

**Why it exists**: Azure requires all resources to belong to a resource group. The resource group allows us to manage all platform resources as a single unit, apply consistent tags, and enforce governance policies.

**Separation of concerns**: Each environment (dev, staging, production) has its own resource group. This prevents accidental cross-environment changes and allows environment-specific access control.

---

### Azure SQL Database

Azure SQL Database is the system of record for all claim data. It hosts the V1 schema defined in Phase 3 and enforces referential integrity, concurrency control, and audit logging.

**Why it exists**: Claims data is structured, relational, and requires ACID transactions. Azure SQL provides enterprise-grade reliability, automated backups, point-in-time restore, and built-in threat detection.

**Separation of concerns**: The database stores only structured claim data. Unstructured documents are stored in Blob Storage. Secrets are stored in Key Vault. The database does not store application code or configuration.

**Security posture**: Public access is disabled. Access is granted via Azure AD authentication and Managed Identity. Transparent Data Encryption (TDE) protects data at rest. Threat detection monitors for suspicious activity.

---

### Azure Blob Storage

Azure Blob Storage is the system of record for all claim documents. It stores PDFs, images, and other unstructured files submitted by claimants or generated during processing.

**Why it exists**: Documents are legal artifacts that may be used as evidence in disputes or litigation. Blob Storage provides durable, scalable, and cost-effective storage with lifecycle management and immutability options.

**Separation of concerns**: Blob Storage stores only file content. Document metadata (filename, upload timestamp, uploader identity) is stored in the ClaimDocuments table in Azure SQL. This separation ensures that the database remains performant and that large files do not bloat the transactional database.

**Lifecycle management**: Documents transition from Hot to Cool to Archive storage tiers over time. This reduces storage costs while maintaining compliance with data retention requirements. Hot tier is used for active claims (0-30 days). Cool tier is used for recent claims (30-365 days). Archive tier is used for historical claims (365+ days).

**Security posture**: Public access is disabled. Access is granted via Managed Identity and RBAC. Secure transfer (HTTPS) is enforced. Blob versioning and soft delete protect against accidental deletion.

---

### Azure Key Vault

Azure Key Vault is the system of record for secrets, keys, and certificates. It stores connection strings, API keys, and other sensitive configuration values that cannot be stored in code or environment variables.

**Why it exists**: Secrets must be protected from unauthorized access and must be rotated regularly. Key Vault provides centralized secret management with access control, audit logging, and automatic rotation.

**Separation of concerns**: Key Vault stores only secrets. Non-secret configuration values (environment names, feature flags, service endpoints) are stored in application settings or Bicep parameters. This separation ensures that secrets are protected while non-sensitive configuration remains accessible.

**Security posture**: Soft delete and purge protection are enabled to prevent accidental or malicious deletion of secrets. RBAC authorization model is used instead of access policies. Access is granted via Managed Identity with least-privilege permissions.

---

### Azure Container Apps

Azure Container Apps is the compute platform for the backend API. It hosts the claims intake API, policy validation service, document processing service, and other backend components.

**Why it exists**: Container Apps provides a fully managed, serverless container platform with built-in autoscaling, load balancing, and blue-green deployments. It eliminates the need to manage Kubernetes clusters or virtual machines.

**Separation of concerns**: Container Apps hosts only application code. Data is stored in Azure SQL and Blob Storage. Secrets are stored in Key Vault. Infrastructure is defined in Bicep. This separation ensures that application deployments do not affect data or infrastructure.

**Scaling and resilience**: Container Apps autoscales based on HTTP traffic, CPU, or custom metrics. Multiple replicas provide high availability. Health probes detect and replace unhealthy containers automatically.

**Security posture**: Containers run with Managed Identity and access Azure SQL, Blob Storage, and Key Vault without credentials. Network isolation can be enforced via Virtual Network integration. HTTPS is enforced for all ingress traffic.

---

### Log Analytics Workspace

Log Analytics Workspace is the centralized logging and monitoring platform. It collects logs from all Azure services and provides query, alerting, and visualization capabilities.

**Why it exists**: Regulatory compliance requires comprehensive audit logging. Operational excellence requires visibility into system health and performance. Log Analytics provides both.

**Separation of concerns**: Log Analytics stores only logs and metrics. Application data is stored in Azure SQL and Blob Storage. Secrets are stored in Key Vault. This separation ensures that logs do not contain sensitive data and that log retention does not affect application data retention.

**Retention and compliance**: Logs are retained according to regulatory requirements (typically 7 years for insurance claims). Retention policies are configured in Bicep and enforced automatically.

---

## Resource Relationships

The following diagram illustrates how Azure services relate to each other:

```
Resource Group
├── Azure SQL Server
│   └── Azure SQL Database (Claims data)
├── Storage Account
│   └── Blob Container (Claim documents)
├── Key Vault (Secrets)
├── Log Analytics Workspace (Logs)
└── Container Apps Environment
    └── Container App (Backend API)
        ├── Managed Identity → Azure SQL (read/write)
        ├── Managed Identity → Blob Storage (read/write)
        └── Managed Identity → Key Vault (read secrets)
```

**Key relationships**:
- Container App uses Managed Identity to access Azure SQL, Blob Storage, and Key Vault
- All services send logs to Log Analytics Workspace
- All services are deployed to the same Resource Group
- All services use Azure AD authentication (no passwords)

---

## Network Topology

**Initial deployment**: All services are accessible via Azure backbone network. Public access is disabled for Azure SQL and Blob Storage. Container Apps ingress is public but can be restricted to specific IP ranges or integrated with Azure Front Door.

**Future enhancement**: Virtual Network integration can be added to fully isolate services. Private Endpoints can be used for Azure SQL, Blob Storage, and Key Vault. This provides additional network-level security but adds complexity.

---

## Identity & Access Management

All service-to-service authentication uses Azure Managed Identity. No connection strings, passwords, or API keys are stored in application code or configuration.

**Managed Identity flow**:
1. Container App is assigned a System-Assigned Managed Identity
2. Azure SQL is configured to allow authentication via Azure AD
3. Blob Storage is configured with RBAC roles (Storage Blob Data Contributor)
4. Key Vault is configured with RBAC roles (Key Vault Secrets User)
5. Container App authenticates using its Managed Identity token

**Human access**: Developers and operators access Azure resources using their Azure AD accounts. Role-Based Access Control (RBAC) is used to grant least-privilege permissions. No shared accounts or service principals with passwords.

---

## Cost Optimization

**Azure SQL**: Sized appropriately for workload. Autoscaling can be enabled if needed. Backup retention is configured to meet compliance requirements without over-retaining.

**Blob Storage**: Lifecycle management moves blobs to cheaper tiers over time. Hot tier for active claims (0-30 days), Cool tier for recent claims (30-365 days), Archive tier for historical claims (365+ days).

**Container Apps**: Autoscales to zero when idle (if appropriate for workload). Scales up based on traffic. No over-provisioning.

**Log Analytics**: Log retention is configured to meet compliance requirements. Older logs can be exported to cheaper storage if needed.

---

## Deployment Strategy

All infrastructure is deployed via Bicep using Azure CLI or Azure DevOps pipelines. Deployments are idempotent and can be run multiple times safely.

**Environments**: Each environment (dev, staging, production) has its own resource group and its own set of resources. Bicep parameters control environment-specific values (SKUs, scaling limits, retention policies).

**Versioning**: Infrastructure changes are versioned in source control. Each deployment is tagged with a version number. Rollback is achieved by redeploying a previous version.

**Validation**: Deployments are validated in dev and staging before being applied to production. Automated tests confirm that resources are healthy and accessible.

---

## Security Posture Summary

- **No public access**: Azure SQL and Blob Storage are not accessible from the public internet
- **No passwords**: All authentication uses Managed Identity or Azure AD
- **Encryption everywhere**: Data at rest is encrypted (TDE for SQL, SSE for Blob). Data in transit is encrypted (HTTPS/TLS)
- **Audit logging**: All access and operations are logged to Log Analytics
- **Least privilege**: RBAC roles grant only the permissions needed for each service
- **Soft delete**: Key Vault and Blob Storage have soft delete enabled to prevent accidental deletion
- **Threat detection**: Azure SQL has threat detection enabled to monitor for suspicious activity

---

## What This Topology Does Not Include (Yet)

- **Azure OpenAI**: Will be added in Phase 6 after backend API is operational
- **Azure Front Door**: Can be added for global load balancing and WAF
- **Virtual Network**: Can be added for network isolation
- **Private Endpoints**: Can be added for additional network security
- **Azure Monitor Alerts**: Can be added for proactive monitoring
- **Azure DevOps Pipelines**: Can be added for CI/CD automation

---

## Related Documents

- **Data Model**: `/docs/data-model.md`
- **Migration Strategy**: `/docs/db-migration-strategy.md`
- **Product Contract**: `/docs/product-contract.md`

---

**Document Owner**: Infrastructure & Platform Engineering  
**Last Updated**: February 2026  
**Next Review**: Q2 2026
