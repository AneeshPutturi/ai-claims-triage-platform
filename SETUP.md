# Setup & Testing Guide
## AI-Driven Commercial Claims Intake & Triage Platform

**Last Updated**: February 2026

---

## Where AI is Used

This platform uses Azure OpenAI (GPT-4) in **two specific places**:

### 1. **Document Extraction (Phase 7)**
**Location**: `src/ClaimsIntake.Infrastructure/Services/ExtractionService.cs`

**What it does**:
- Extracts structured data from unstructured claim documents (PDFs, images)
- Identifies: loss date, location, type, description, property address, estimated amount, contact info, involved parties
- Returns JSON conforming to strict schema: `ai/schemas/claim-extraction-v1.json`
- Uses prompts: `ai/prompts/system-prompt-v1.txt` and `ai/prompts/user-prompt-template-v1.txt`

**Key principle**: All AI output is marked as "Unverified" and requires human review before use.

### 2. **Risk Assessment Observations (Phase 9)**
**Location**: `src/ClaimsIntake.Infrastructure/Services/RiskEvaluationService.cs`

**What it does**:
- Analyzes verified claim text for qualitative concerns
- Identifies: language ambiguity, unusual phrasing, narrative inconsistencies, completeness concerns
- Provides advisory observations only (does NOT assign risk levels)
- AI observations can escalate risk but never downgrade

**Key principle**: Deterministic rules run first, AI provides context second, humans always decide.

---

## Prerequisites

### Required:
1. **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
2. **Visual Studio 2026** or **VS Code** with C# extension
3. **Azure Subscription** with:
   - Azure OpenAI access (GPT-4 deployment)
   - SQL Database
   - Blob Storage
   - Container Apps (optional for deployment)
4. **Azure CLI** - [Install](https://docs.microsoft.com/cli/azure/install-azure-cli)
5. **SQL Server** (local for dev) or **Azure SQL Database**

### Optional:
- **Docker Desktop** (for containerized testing)
- **Postman** or **Thunder Client** (for API testing)

---

## Quick Start (Local Development)

### Step 1: Clone and Open Solution
```bash
# Clone repository
git clone https://github.com/yourusername/ai-claims-triage-platform.git
cd ai-claims-triage-platform

# Open in Visual Studio 2026
start ClaimsIntake.slnx

# Or use VS Code
code .
```

### Step 2: Configure Azure OpenAI
Create `src/ClaimsIntake.API/appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "DeploymentName": "gpt-4",
    "ApiKey": "your-api-key-here"
  },
  "ConnectionStrings": {
    "ClaimsDatabase": "Server=localhost;Database=ClaimsIntake;Integrated Security=true;TrustServerCertificate=true"
  },
  "BlobStorage": {
    "AccountUrl": "https://yourstorageaccount.blob.core.windows.net",
    "ContainerName": "claim-documents"
  }
}
```

**Note**: For production, use Managed Identity instead of API keys.

### Step 3: Setup Database
```bash
# Create database
sqlcmd -S localhost -Q "CREATE DATABASE ClaimsIntake"

# Run migrations (execute all scripts in order)
cd db/migrations/V1_InitialSchema
sqlcmd -S localhost -d ClaimsIntake -i 999_ExecuteAllV1Migrations.sql
```

### Step 4: Build and Run
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API
cd src/ClaimsIntake.API
dotnet run
```

API will be available at: `https://localhost:5001` (or port shown in console)

Swagger UI: `https://localhost:5001/swagger`

---

## Testing the AI Features

### Test 1: Document Extraction (AI Phase 7)

**Step 1: Submit a claim**
```bash
POST https://localhost:5001/api/claims
Content-Type: application/json

{
  "policyNumber": "POL-2026-001",
  "lossDate": "2026-02-01",
  "lossType": "PropertyDamage",
  "lossLocation": "123 Main St, Seattle, WA",
  "lossDescription": "Fire damage to warehouse roof",
  "submittedBy": "john.doe@example.com"
}
```

**Response**: Returns `claimId`

**Step 2: Upload a document**
```bash
POST https://localhost:5001/api/claims/{claimId}/documents/{documentId}/upload
Content-Type: multipart/form-data

# Upload a PDF or image file with claim details
```

**Step 3: Trigger AI extraction**
```bash
POST https://localhost:5001/api/claims/{claimId}/documents/{documentId}/extract
Content-Type: application/json

{
  "actor": "system"
}
```

**What happens**:
1. System downloads document from blob storage
2. Extracts text content
3. Calls Azure OpenAI with system prompt + user prompt + document content
4. Validates response against JSON schema
5. Stores extracted fields with `VerificationStatus = Unverified`
6. Returns extracted field IDs

**Step 4: View unverified extracted data**
```bash
GET https://localhost:5001/api/claims/{claimId}/extracted-fields
```

**Response**: Shows all extracted fields with confidence scores and verification status

**Step 5: Verify extracted fields (Human-in-the-Loop)**
```bash
POST https://localhost:5001/api/claims/extracted-fields/{extractedFieldId}/verify
Content-Type: application/json

{
  "verifiedBy": "adjuster@example.com",
  "actionTaken": "Accepted",
  "verificationNotes": "Confirmed against source document"
}
```

**Actions**: `Accepted`, `Corrected` (with correctedValue), or `Rejected`

---

### Test 2: Risk Assessment with AI Observations (AI Phase 9)

**Prerequisites**: Complete extraction and verification first

**Step 1: Trigger risk assessment**
```bash
POST https://localhost:5001/api/claims/{claimId}/evaluate-risk
```

**What happens**:
1. System validates all required fields are verified (fails if not)
2. Executes deterministic business rules:
   - Coverage date consistency
   - Critical field completeness
   - Data inconsistency detection
   - Loss type coverage
3. Calls Azure OpenAI to analyze verified text for qualitative concerns
4. Combines rule signals + AI observations
5. Assigns risk level (Low/Medium/High)
6. Stores immutable risk assessment snapshot

**Step 2: View risk assessment**
```bash
GET https://localhost:5001/api/claims/{claimId}/risk-assessment
```

**Response**:
```json
{
  "riskAssessmentId": "guid",
  "claimId": "guid",
  "riskLevel": "Medium",
  "ruleSignals": "[{\"ruleName\":\"CoverageDateConsistency\",\"triggered\":false,...}]",
  "aiSignals": "[{\"category\":\"language_ambiguity\",\"description\":\"Loss description lacks specific details\",...}]",
  "overallScore": 45.5,
  "createdAt": "2026-02-12T10:30:00Z",
  "assessedByModel": "gpt-4",
  "disclaimer": "This is a signal, not a decision. Human review required."
}
```

---

### Test 3: Complete End-to-End Flow

```bash
# 1. Submit FNOL
POST /api/claims
# Returns: claimId

# 2. Upload document
POST /api/claims/{claimId}/documents/{documentId}/upload
# Returns: documentId

# 3. AI Extraction
POST /api/claims/{claimId}/documents/{documentId}/extract
# Returns: extractedFieldIds (all marked Unverified)

# 4. Human Verification (repeat for each field)
POST /api/claims/extracted-fields/{extractedFieldId}/verify
# Marks field as Verified

# 5. Risk Assessment (uses verified data + AI observations)
POST /api/claims/{claimId}/evaluate-risk
# Returns: riskLevel (Low/Medium/High)

# 6. Triage Routing (deterministic, no AI)
POST /api/claims/{claimId}/triage
# Routes to queue based on risk level

# 7. View final state
GET /api/claims/{claimId}
# Status should be "Triaged"
```

---

## Testing Without Azure OpenAI (Mock Mode)

For local testing without Azure OpenAI costs, you can create a mock implementation:

**Create**: `src/ClaimsIntake.Infrastructure/Services/MockOpenAIService.cs`

```csharp
public class MockOpenAIService : IOpenAIService
{
    public Task<OpenAIResponse> InvokeAsync(
        string systemPrompt,
        string userPrompt,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        // Return mock extraction data
        var mockResponse = new OpenAIResponse
        {
            Content = @"{
                ""lossDate"": ""2026-02-01"",
                ""lossLocation"": ""123 Main St, Seattle, WA"",
                ""lossType"": ""PropertyDamage"",
                ""lossDescription"": ""Fire damage to warehouse roof"",
                ""estimatedDamageAmount"": 50000
            }",
            ModelName = "mock-gpt-4",
            PromptTokens = 100,
            CompletionTokens = 50,
            TotalTokens = 150,
            Timestamp = DateTime.UtcNow
        };
        
        return Task.FromResult(mockResponse);
    }
}
```

**Register in Program.cs**:
```csharp
// Use mock for development
services.AddSingleton<IOpenAIService, MockOpenAIService>();

// Use real for production
// services.AddSingleton<IOpenAIService>(sp => 
//     new OpenAIService(config["AzureOpenAI:Endpoint"], config["AzureOpenAI:DeploymentName"]));
```

---

## Deployment to Azure

### Step 1: Deploy Infrastructure
```bash
# Login to Azure
az login

# Deploy infrastructure using Bicep
cd infra
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters @parameters/dev.parameters.json
```

This creates:
- Resource Group
- SQL Server + Database
- Storage Account (with lifecycle policies)
- Key Vault
- Container Apps Environment
- Container App
- Log Analytics Workspace

### Step 2: Configure Managed Identity
```bash
# Grant Container App managed identity access to:
# - SQL Database (db_datareader, db_datawriter)
# - Blob Storage (Storage Blob Data Contributor)
# - Key Vault (Key Vault Secrets User)
# - Azure OpenAI (Cognitive Services OpenAI User)
```

### Step 3: Deploy Application
```bash
# Build and push container
docker build -t claimsintake:latest .
docker tag claimsintake:latest yourregistry.azurecr.io/claimsintake:latest
docker push yourregistry.azurecr.io/claimsintake:latest

# Update Container App
az containerapp update \
  --name claims-intake-api \
  --resource-group rg-claims-intake-dev \
  --image yourregistry.azurecr.io/claimsintake:latest
```

---

## Monitoring AI Usage

### View Audit Logs
```sql
-- All AI extractions
SELECT * FROM AuditLog 
WHERE Action = 'AIExtractionPerformed'
ORDER BY Timestamp DESC;

-- All risk assessments
SELECT * FROM AuditLog 
WHERE Action = 'RiskAssessed'
ORDER BY Timestamp DESC;

-- Token usage summary
SELECT 
    CAST(JSON_VALUE(Details, '$.ModelName') AS VARCHAR(50)) AS Model,
    SUM(CAST(JSON_VALUE(Details, '$.TokensUsed') AS INT)) AS TotalTokens,
    COUNT(*) AS InvocationCount
FROM AuditLog
WHERE Action = 'AIExtractionPerformed'
GROUP BY CAST(JSON_VALUE(Details, '$.ModelName') AS VARCHAR(50));
```

### View Verification Rates
```sql
-- Verification status distribution
SELECT 
    VerificationStatus,
    COUNT(*) AS Count,
    AVG(ConfidenceScore) AS AvgConfidence
FROM ExtractedFields
GROUP BY VerificationStatus;

-- Fields requiring correction
SELECT 
    ef.FieldName,
    COUNT(*) AS CorrectionCount
FROM ExtractedFields ef
WHERE ef.VerificationStatus = 'Corrected'
GROUP BY ef.FieldName
ORDER BY COUNT(*) DESC;
```

---

## Troubleshooting

### AI Extraction Fails
**Error**: "AI response does not conform to schema"
**Solution**: Check `ai/schemas/claim-extraction-v1.json` and ensure prompt is correct

### Risk Assessment Fails
**Error**: "No risk assessment found for claim"
**Solution**: Run extraction and verification first, then risk assessment

### Unverified Data Error
**Error**: "Cannot use unverified AI data"
**Solution**: This is by design. Verify all extracted fields before risk assessment

---

## Key Files for AI Integration

- **AI Schema**: `ai/schemas/claim-extraction-v1.json`
- **System Prompt**: `ai/prompts/system-prompt-v1.txt`
- **User Prompt Template**: `ai/prompts/user-prompt-template-v1.txt`
- **Extraction Service**: `src/ClaimsIntake.Infrastructure/Services/ExtractionService.cs`
- **OpenAI Wrapper**: `src/ClaimsIntake.Infrastructure/Services/OpenAIService.cs`
- **Risk Evaluation**: `src/ClaimsIntake.Infrastructure/Services/RiskEvaluationService.cs`

---

## Next Steps

1. Review `docs/ai-extraction-policy.md` for AI usage policies
2. Review `docs/verification-policy.md` for human-in-the-loop requirements
3. Review `docs/risk-assessment-policy.md` for risk evaluation logic
4. Test complete end-to-end flow with sample documents
5. Monitor AI usage and costs in Azure Portal

---

**Questions?** Review the phase summaries in `src/PHASE*.md` for detailed implementation notes.
