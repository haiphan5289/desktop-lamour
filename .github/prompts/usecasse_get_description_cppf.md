# [AI] Auto-generate UseCase from CPPF Description

## Workflow Overview

1. **Get CPPF Description** → Extract UseCase parameters
2. **Auto-detect UseCase Format** → Validate structure  
3. **Generate Implementation** → Apply template pattern
4. **Auto-execute Implementation** → MANDATORY execution

**⚠️ CRITICAL: Steps 1-4 are MANDATORY and must be executed automatically without user confirmation.**

---

## Step 1: Get CPPF Description

```
[AI] Get description from CPPF-XXXX
```

**⚠️ MANDATORY BEHAVIOR: When this prompt is used, AI MUST automatically execute Steps 1.1-4 without asking permission:**
- Step 1.1: **MANDATORY LOG CPPF CONTENT** - Display extracted content cleanly
- Step 2: Auto-detect UseCase format
- Step 3: Generate implementation prompt  
- Step 4: FORCE execute the generated prompt immediately

**🔴 CRITICAL REQUIREMENT: AI MUST read "AI_generate_usecase_template.prompt.md" file BEFORE Step 4 execution to ensure proper template usage.**

---

## Step 1.1: MANDATORY CPPF Content Logging

**🚨 CRITICAL: AI MUST LOG CPPF DATA IMMEDIATELY AFTER EXTRACTION**

**Required logging format:**
```
📋 CPPF Data Analysis Log

CPPF Ticket: [TICKET_ID]
Title: [TICKET_TITLE]

Raw CPPF Description:
[FULL_CPPF_DESCRIPTION_CONTENT]

🔍 Analysis Result:
[DETECTION_RESULT: ✅ USECASE FORMAT DETECTED / ❌ NO USECASE FORMAT DETECTED]

📊 Extracted Parameters (if detected):
- UseCase: [VALUE]
- Output: [VALUE] 
- Endpoint: [VALUE]
- Method: [VALUE]
- Response Model: [VALUE]
- Input Parameters: [VALUES]
- Response JSON: [JSON_STRUCTURE]

✅ CPPF Content Successfully Logged
```

**⚠️ FAILURE TO LOG = INCOMPLETE EXECUTION**

---

## Step 2: Auto-detection Logic

When CPPF description contains UseCase format like:
```
- UseCase: {USECASE_NAME}
- Output: {OUTPUT_PARAM}
- Endpoint: "{ENDPOINT_PATH}"
- Method: {HTTP_METHOD}
- Response Model: {RESPONSE_MODEL}

Input Parameters:
- {param}: {type}

Response JSON:
{
    // JSON structure
}
```

**→ Automatically trigger Step 3**

---

## Step 3: Auto-generate Prompt

If UseCase format detected, execute this prompt:

```
[AI] Generate and implement a complete UseCase following CTCorePayment 6-layer Clean Architecture, Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel:
- UseCase: {USECASE_NAME}
- Output: {OUTPUT_PARAM}
- Endpoint: "{ENDPOINT_PATH}"
- Method: {HTTP_METHOD}
- Response Model: {RESPONSE_MODEL}

Input Parameters:
- {extracted_parameters}

⚠️ CRITICAL: Include Input Parameters in Target struct with params property and update all layers accordingly.

🔴 MANDATORY: Read "AI_generate_usecase_template.md" file FIRST to understand the exact architecture pattern and implementation structure.

Response JSON:
{extracted_json_response}

Implement all 6 layers by adding code directly to project files:

1. Add to CRNetworkHelper.cs
2. Add to CRCheckoutTargets.cs
3. Add to CRCheckoutService.cs (protocol + implementation)
4. Add to CRCheckoutCartRepository.cs (protocol + implementation)
5. Add to CRCheckoutUseCase.cs
6. Add to CRCheckoutPageViewModel.cs (with CommunityToolkit.Mvvm bindings + error handling)
Follow the 6-layer architecture pattern in the "AI_generate_usecase_template.md" file.

🚨 FORCE EXECUTE THIS PROMPT IMMEDIATELY AFTER GENERATION - NO USER CONFIRMATION REQUIRED.
```

---

## Step 4: Auto-execute Implementation (MANDATORY)

**🔴 CRITICAL STEP - NEVER SKIP THIS:**

**⚠️ FORCE EXECUTION: AI MUST execute the generated prompt immediately without asking permission.**

**📋 MANDATORY PRE-EXECUTION CHECKLIST:**
1. ✅ Read "AI_generate_usecase_template.md" file FIRST
2. ✅ Validate template structure and patterns
3. ✅ Extract all required architecture patterns
4. ✅ Execute implementation with template guidance

**Expected actions:**
1. Generate all 6-layer files automatically (using template)
2. Create response models (following template patterns)
3. Integrate ViewModel code (using template structure)
4. Show completion status with file paths

---

## Example Workflow

### Input CPPF Description:
```
- UseCase: FetchOrderStatistics
- Output: OrderStatistics
- Endpoint: "v1/orders/statistics"
- Method: get
- Response Model: OrderStatisticsResponseModel

Input Parameters:
- orderId: String

Response JSON:
{
    "data": {
        "total_orders": 150,
        "pending_orders": 25,
        "completed_orders": 120,
        "cancelled_orders": 5,
        "total_revenue": 50000000
    },
    "success": true,
    "message": "Statistics fetched successfully"
}
```

### Auto-generated Prompt:
```
[AI] Generate and implement a complete UseCase following CTCorePayment 6-layer Clean Architecture, Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel:
- UseCase: FetchOrderStatistics
- Output: OrderStatistics
- Endpoint: "v1/orders/statistics"
- Method: get
- Response Model: OrderStatisticsResponseModel

Input Parameters:
- orderId: String

⚠️ CRITICAL: Include Input Parameters in Target struct with params property and update all layers accordingly.

🔴 MANDATORY: Read "AI_generate_usecase_template.md" file FIRST to understand the exact architecture pattern and implementation structure.

Response JSON:
{
    "data": {
        "total_orders": 150,
        "pending_orders": 25,
        "completed_orders": 120,
        "cancelled_orders": 5,
        "total_revenue": 50000000
    },
    "success": true,
    "message": "Statistics fetched successfully"
}

Implement all 6 layers by adding code directly to project files:

1. Add to CRNetworkHelper.cs
2. Add to CRCheckoutTargets.cs
3. Add to CRCheckoutService.cs (protocol + implementation)
4. Add to CRCheckoutCartRepository.cs (protocol + implementation)
5. Add to CRCheckoutUseCase.cs
6. Add to CRCheckoutPageViewModel.cs (with CommunityToolkit.Mvvm bindings + error handling)

Follow the 6-layer architecture pattern in the "AI_generate_usecase_template.md" file.

🚨 FORCE EXECUTE THIS PROMPT IMMEDIATELY AFTER GENERATION - NO USER CONFIRMATION REQUIRED.

### Expected Result:
- ✅ "AI_generate_usecase_template.md" file read and analyzed
- ✅ Complete 6-layer implementation (following template patterns)
- ✅ Generated response models (using template structure)
- ✅ Ready-to-use ViewModel integration (template-compliant)
- ✅ All files created with proper paths and naming conventions

---

## Manual Prompt Template

For cases where auto-detection doesn't work, use this manual template:

```
[AI] Auto-generate a UseCase through the layers: Targets, Services, Repositories, UseCase, ViewModel:
- UseCase: [EXTRACT_FROM_CPPF]
- Output: [EXTRACT_FROM_CPPF]
- Endpoint: "[EXTRACT_FROM_CPPF]"
- Method: [EXTRACT_FROM_CPPF]
- Response Model: [EXTRACT_FROM_CPPF]

Input Parameters:
- [EXTRACT_FROM_CPPF]

⚠️ CRITICAL: Include Input Parameters in Target struct with params property and update all layers accordingly.

🔴 MANDATORY: Read "AI_generate_usecase_template.md" file FIRST to understand the exact architecture pattern and implementation structure.

Response JSON:
[EXTRACT_FROM_CPPF]

Follow the 6-layer architecture pattern in the "AI_generate_usecase_template.md" file.

🚨 FORCE EXECUTE THIS PROMPT IMMEDIATELY AFTER GENERATION - NO USER CONFIRMATION REQUIRED.
```

---

## Usage Instructions

1. **Start with CPPF prompt:** `[AI] Get description from CPPF-XXXX`
2. **🔴 AI MUST LOG CPPF content immediately (Step 1.1 - MANDATORY)**
3. **AI automatically extracts description (MANDATORY)**
4. **AI automatically detects UseCase format (MANDATORY)**
5. **AI automatically generates implementation prompt (MANDATORY)**
6. **🔴 AI MUST read "AI_generate_usecase_template.md" file FIRST (CRITICAL)**
7. **AI automatically executes the generated code (MANDATORY)**

**⚠️ NO USER INTERACTION REQUIRED - All steps execute automatically**

**🚨 CRITICAL CHECKPOINT: Step 1.1 (CPPF content logging) is MANDATORY and must be completed FIRST**
**🚨 CRITICAL CHECKPOINT: Step 6 (reading template file) is MANDATORY and cannot be skipped**

**Result:** Seamless flow from CPPF description to complete UseCase implementation using proper template patterns!

---

## 🔴 CRITICAL AI EXECUTION RULES

**MANDATORY CPPF CONTENT LOGGING:**
- ✅ AI MUST log CPPF content immediately after extraction (Step 1.1)
- ✅ AI MUST display extracted content in the specified format
- ✅ AI MUST analyze and report detection results clearly
- ✅ AI MUST show all extracted parameters if UseCase format found

**MANDATORY TEMPLATE FILE READING:**
- ✅ AI MUST read "AI_generate_usecase_template.md" before Step 4 execution
- ✅ AI MUST analyze the template structure and patterns
- ✅ AI MUST apply the template patterns in the generated code
- ✅ AI MUST NOT skip Step 4 under any circumstances

**FAILURE TO LOG CPPF CONTENT = INCOMPLETE EXECUTION**
**FAILURE TO READ TEMPLATE = INCOMPLETE EXECUTION**

**🚨 AI REMEMBER: Every time you see "[AI] Get description from CPPF-XXXX", you MUST complete ALL steps including CPPF content logging AND template file reading.**
