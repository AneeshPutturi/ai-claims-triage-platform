#!/bin/bash
# =============================================
# Infrastructure Security Validation Script
# Description: Validates security posture after deployment
# Author: Infrastructure Team
# Date: February 2026
# =============================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parameters
ENVIRONMENT=${1:-dev}
RESOURCE_GROUP="rg-claims-intake-${ENVIRONMENT}"

echo "========================================="
echo "Infrastructure Security Validation"
echo "Environment: ${ENVIRONMENT}"
echo "Resource Group: ${RESOURCE_GROUP}"
echo "========================================="
echo ""

# Check if resource group exists
if ! az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    echo -e "${RED}✗ Resource group not found: ${RESOURCE_GROUP}${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Resource group exists${NC}"
echo ""

# Get resource names
SQL_SERVER=$(az sql server list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)
STORAGE_ACCOUNT=$(az storage account list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)
KEY_VAULT=$(az keyvault list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)

echo "Resources found:"
echo "  SQL Server: ${SQL_SERVER}"
echo "  Storage Account: ${STORAGE_ACCOUNT}"
echo "  Key Vault: ${KEY_VAULT}"
echo ""

# =============================================
# VALIDATE SQL SERVER SECURITY
# =============================================

echo "Validating SQL Server security..."

# Check public network access
PUBLIC_ACCESS=$(az sql server show --name "$SQL_SERVER" --resource-group "$RESOURCE_GROUP" --query publicNetworkAccess -o tsv)
if [ "$PUBLIC_ACCESS" == "Disabled" ]; then
    echo -e "${GREEN}✓ SQL Server public access is disabled${NC}"
else
    echo -e "${YELLOW}⚠ SQL Server public access is enabled (consider disabling for production)${NC}"
fi

# Check TLS version
TLS_VERSION=$(az sql server show --name "$SQL_SERVER" --resource-group "$RESOURCE_GROUP" --query minimalTlsVersion -o tsv)
if [ "$TLS_VERSION" == "1.2" ]; then
    echo -e "${GREEN}✓ SQL Server requires TLS 1.2${NC}"
else
    echo -e "${RED}✗ SQL Server TLS version is not 1.2${NC}"
fi

# Check auditing
AUDITING=$(az sql server audit-policy show --name "$SQL_SERVER" --resource-group "$RESOURCE_GROUP" --query state -o tsv)
if [ "$AUDITING" == "Enabled" ]; then
    echo -e "${GREEN}✓ SQL Server auditing is enabled${NC}"
else
    echo -e "${RED}✗ SQL Server auditing is not enabled${NC}"
fi

# Check threat detection
THREAT_DETECTION=$(az sql server threat-policy show --name "$SQL_SERVER" --resource-group "$RESOURCE_GROUP" --query state -o tsv)
if [ "$THREAT_DETECTION" == "Enabled" ]; then
    echo -e "${GREEN}✓ SQL Server threat detection is enabled${NC}"
else
    echo -e "${RED}✗ SQL Server threat detection is not enabled${NC}"
fi

echo ""

# =============================================
# VALIDATE STORAGE ACCOUNT SECURITY
# =============================================

echo "Validating Storage Account security..."

# Check HTTPS only
HTTPS_ONLY=$(az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" --query supportsHttpsTrafficOnly -o tsv)
if [ "$HTTPS_ONLY" == "true" ]; then
    echo -e "${GREEN}✓ Storage Account requires HTTPS${NC}"
else
    echo -e "${RED}✗ Storage Account does not require HTTPS${NC}"
fi

# Check public blob access
PUBLIC_BLOB=$(az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" --query allowBlobPublicAccess -o tsv)
if [ "$PUBLIC_BLOB" == "false" ]; then
    echo -e "${GREEN}✓ Storage Account public blob access is disabled${NC}"
else
    echo -e "${RED}✗ Storage Account public blob access is enabled${NC}"
fi

# Check TLS version
STORAGE_TLS=$(az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" --query minimumTlsVersion -o tsv)
if [ "$STORAGE_TLS" == "TLS1_2" ]; then
    echo -e "${GREEN}✓ Storage Account requires TLS 1.2${NC}"
else
    echo -e "${RED}✗ Storage Account TLS version is not 1.2${NC}"
fi

# Check blob versioning
VERSIONING=$(az storage account blob-service-properties show --account-name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" --query isVersioningEnabled -o tsv)
if [ "$VERSIONING" == "true" ]; then
    echo -e "${GREEN}✓ Blob versioning is enabled${NC}"
else
    echo -e "${YELLOW}⚠ Blob versioning is not enabled${NC}"
fi

echo ""

# =============================================
# VALIDATE KEY VAULT SECURITY
# =============================================

echo "Validating Key Vault security..."

# Check soft delete
SOFT_DELETE=$(az keyvault show --name "$KEY_VAULT" --resource-group "$RESOURCE_GROUP" --query properties.enableSoftDelete -o tsv)
if [ "$SOFT_DELETE" == "true" ]; then
    echo -e "${GREEN}✓ Key Vault soft delete is enabled${NC}"
else
    echo -e "${RED}✗ Key Vault soft delete is not enabled${NC}"
fi

# Check purge protection
PURGE_PROTECTION=$(az keyvault show --name "$KEY_VAULT" --resource-group "$RESOURCE_GROUP" --query properties.enablePurgeProtection -o tsv)
if [ "$PURGE_PROTECTION" == "true" ]; then
    echo -e "${GREEN}✓ Key Vault purge protection is enabled${NC}"
else
    echo -e "${RED}✗ Key Vault purge protection is not enabled${NC}"
fi

# Check RBAC authorization
RBAC=$(az keyvault show --name "$KEY_VAULT" --resource-group "$RESOURCE_GROUP" --query properties.enableRbacAuthorization -o tsv)
if [ "$RBAC" == "true" ]; then
    echo -e "${GREEN}✓ Key Vault uses RBAC authorization${NC}"
else
    echo -e "${YELLOW}⚠ Key Vault uses access policies (consider migrating to RBAC)${NC}"
fi

echo ""

# =============================================
# SUMMARY
# =============================================

echo "========================================="
echo "Security Validation Complete"
echo "========================================="
echo ""
echo "Review any warnings or errors above."
echo "For production environments, all checks should pass."
echo ""
