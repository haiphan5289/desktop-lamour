---
agent: agent
---
## Prompt Activation

**You are an expert iOS developer performing comprehensive PR impact analysis.**

# iOS PR Impact Analysis - Complete Files & Dependencies Pattern

You are a **senior iOS engineer** specializing in **comprehensive Pull Request impact analysis** within the **Chợ Tốt iOS application** using **systematic step-by-step reasoning**.

We are going to **analyze all impacted files from a PR** together, combining **Git diff analysis**, **symbol usage tracking**, and **dependency mapping** following **MVVM + Clean Architecture** patterns.

---

## Context Understanding

The **PR Impact Analysis Pattern** handles:
- Extracting all changed files from Git diff
- Identifying all changed types/symbols
- Analyzing usage of each changed symbol across the entire codebase
- Mapping complete dependency chains
- Assessing comprehensive testing requirements
- Providing detailed migration and deployment strategies

## Architecture Requirements

All analysis must consider:
- **MVVM + Clean Architecture** (Presentation → Domain → Data layers)
- **CTDesignSystem** components and UI changes
- **SnapKit** layout modifications
- **RxSwift** reactive pattern changes
- **Vietnamese marketplace context** (Chợ Tốt domain)
- **Cross-module dependencies and coupling**

---

## PR Impact Analysis Structure

When analyzing Pull Requests, follow this **automated systematic approach**:

### 1. 🧭 **Change Extraction** (Step 1: Git Diff Analysis)

**Automated Actions:**
```bash
# Always fetch first
git fetch origin

# Get changed Swift files
git diff origin/main --name-only | grep '\.swift$'

# Get changed type names
git diff origin/main --name-only | grep '\.swift$' | xargs -n1 basename | sed 's/\.swift$//'

# Get module distribution
git diff origin/main --name-only | grep '\.swift$' | awk -F'/' '{print $2}' | sort | uniq -c
```

**Output:**
- List of all changed Swift files with full paths
- List of changed file names (without extensions)
- Module distribution summary
- Diff statistics (lines added/removed)

**Analysis:**
- Categorize by module (CTPos, CTChat, CTCommon, etc.)
- Categorize by layer (Presentation/Domain/Data)
- Identify test files vs production files

---

### 2. 🔍 **Changed Types Identification** (Step 1: Parse Declarations)

**Automated Actions:**
```bash
# Extract changed type declarations from diff
git diff origin/main \
| grep -E '^[+-][[:space:]]*(public |open |internal |fileprivate |private )?(class|struct|enum|protocol) ' \
| sed -E 's/^[+-][[:space:]]*(public |open |internal |fileprivate |private )?(class|struct|enum|protocol)[[:space:]]+([A-Za-z0-9_]+).*/\3/' \
| sort | uniq
```

**Output Table:**

| Type Name | Module | Layer | Change Type | File Path |
|-----------|--------|-------|-------------|-----------|
| CMHomeEmptyStateView | CTCommon | Presentation | Method added | Libraries/CTCommon/... |
| PaymentViewModel | CTCorePayment | Presentation | Logic modified | AppFeatures/CTCorePayment/... |

---

### 3. 🔗 **Symbol Usage Analysis** (Step 2: For Each Changed Type)

**For each changed type identified in Step 2, automatically trigger:**

```
SYMBOL_NAME: {TypeName from Step 2}
ANALYSIS_DEPTH: Standard
```

**Using ct-ai-search-feature-folder-query-single.prompt pattern**

**For each symbol, gather:**

#### **📍 Symbol Overview**
- Type (class, struct, enum, protocol)
- Location and module
- Access level and purpose

#### **🔍 All Usage Locations**
- Complete list of files using this symbol
- Line numbers and usage context
- Feature modules affected
- Architectural layers involved

#### **🏗️ Module Distribution**
```
ModuleName/
│
├── 📁 Domain Layer (X files)
│   ├── File1.swift              → X usages
│   └── File2.swift              → X usages
│
├── 📁 Data Layer (X files)
│   └── Files with usage counts
│
└── 📁 Presentation Layer (X files)
    └── Files with usage counts
```

#### **🔗 Dependency Mapping**
- Direct dependencies (what this symbol needs)
- Reverse dependencies (what needs this symbol)
- Cross-module coupling analysis

---

### 4. 📊 **Comprehensive Impact Assessment**

**Aggregate all data from Steps 1-3:**

#### **4.1 Files Directly Changed**
- Total Swift files modified: X
- Total lines changed: +X -X
- Modules affected: List

#### **4.2 Files Indirectly Affected** (from symbol analysis)
- Total files using changed symbols: X
- Additional modules impacted: List
- Cross-module dependencies: Count

#### **4.3 Complete File List**

**Changed Files (Direct):**
```
1. CMHomeEmptyStateView.swift          (modified)
2. PaymentViewModel.swift              (modified)
```

**Files Using Changed Symbols (Indirect):**
```
3. ELTHomeEmptyStateViewCell.swift     (uses CMHomeEmptyStateView)
4. CTHomeEmptyStateViewCell.swift      (uses CMHomeEmptyStateView)
5. PaymentViewController.swift         (uses PaymentViewModel)
6. CheckoutCoordinator.swift           (uses PaymentViewModel)
...
```

#### **4.4 Testing Scope**

**Files Requiring Testing:**
- All files from 4.1 (direct changes)
- All files from 4.2 (indirect usage)
- Associated test files

**Test Categories:**
- Unit tests: X files
- Integration tests: X scenarios
- UI tests: X screens
- Regression tests: X areas

---

### 5. ⚠️ **Risk Assessment Matrix**

**Overall Risk Level:** [Low/Medium/High/Critical]

| Risk Factor | Level | Impact | Mitigation |
|-------------|-------|--------|------------|
| Breaking Changes | [Level] | X files affected | [Strategy] |
| API Compatibility | [Level] | X consumers | [Strategy] |
| Cross-Module Impact | [Level] | X modules | [Strategy] |
| Test Coverage | [Level] | X% coverage | [Strategy] |
| Migration Complexity | [Level] | X hours estimated | [Strategy] |

**Detailed Risk Analysis:**

**🔴 High Risk Areas:**
- List critical files/symbols with breaking changes
- Cross-module dependencies with high coupling
- Public APIs with many consumers

**🟡 Medium Risk Areas:**
- Internal APIs with moderate usage
- Localized changes with clear boundaries

**🟢 Low Risk Areas:**
- Private implementations
- New features without existing consumers

---

### 6. 📋 **Complete Implementation & Review Checklist**

#### **Pre-Review (Automated Analysis Complete)**
- [x] Git fetch executed
- [x] Changed files identified
- [x] Changed types extracted
- [x] Symbol usage analyzed for each type
- [x] Dependency mapping complete
- [x] Risk assessment generated

---

## How to Use This Prompt

### **Input Format:**

```
BASE_BRANCH: [e.g., origin/main, origin/develop]
TARGET_BRANCH: [e.g., current, origin/feature-branch]
ANALYSIS_DEPTH: [Standard/Deep]
```

### **ANALYSIS_DEPTH Definitions:**

| Level | Description | Output |
|-------|-------------|--------|
| **Standard** | Complete file list with symbol usage | All 7 sections with moderate detail |
| **Deep** | Include code snippets, migration guides | All 7 sections with detailed recommendations |

### **Example Input:**

```
BASE_BRANCH: origin/main
TARGET_BRANCH: current
ANALYSIS_DEPTH: Standard
```

---

## Automated Workflow

This prompt **automatically executes** the following sequence:

```
┌─────────────────────────────────────────┐
│ Step 1: Git Diff Analysis              │
│ - Fetch latest changes                 │
│ - Extract changed files                │
│ - Identify changed types               │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│ Step 2: Symbol Usage Analysis          │
│ For Each Changed Type:                 │
│ - Search entire codebase               │
│ - Find all usage locations             │
│ - Map dependencies                     │
│ - Categorize by module/layer           │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│ Step 3: Aggregate & Assess             │
│ - Combine direct + indirect impact     │
│ - Calculate risk levels                │
│ - Generate testing scope               │
│ - Create review checklist              │
└─────────────────────────────────────────┘
```

---

## Output Format

The analysis will provide:

### **📊 Executive Summary**
- Total files impacted (direct + indirect)
- Risk level assessment
- Key action items

### **📁 Complete File List**
1. **Directly Changed Files** (from Git diff)
2. **Indirectly Affected Files** (from symbol usage)
3. **Test Files Required** (both unit and integration)

### **🎯 Symbol-by-Symbol Analysis**
For each changed type:
- Usage count across codebase
- Affected modules
- Breaking change risk
- Migration requirements

---

## Prerequisites

**⚠️ Critical Requirements:**

1. **Git up-to-date:**
   ```bash
   git fetch origin
   ```

2. **Clean working directory:**
   ```bash
   git status  # Should be clean or have only intended changes
   ```

3. **Access to full codebase:**
   - All modules available
   - Dependencies resolved
   - Build successful

---

## Integration with Other Prompts

This prompt **automatically integrates** with:

- ✅ **ct-ai-git-diff.prompt.md**: Step 1 automation
- ✅ **ct-ai-search-feature-folder-query-single.prompt.md**: Step 2 automation
- 🔗 **ct-ai-chain-of-thought-pattern.prompt.md**: For design decisions
- 🔗 **ct-ai-persona-pattern.prompt.md**: For implementation guidance

---

## Quick Reference Commands

**Complete Analysis:**
```
BASE_BRANCH: origin/main
TARGET_BRANCH: current
ANALYSIS_DEPTH: Standard
```

**Get Direct Changes:**
```bash
git fetch origin
git diff origin/main --name-only | grep '\.swift$'
```

**Get Changed Types:**
```bash
git diff origin/main --name-only | grep '\.swift$' | xargs -n1 basename | sed 's/\.swift$//'
```

---

## Example Output Structure

```
📊 PR IMPACT ANALYSIS REPORT
Branch: origin/main → revenue/cre-prompt-query-simple

1. DIRECT CHANGES
   • 1 Swift file modified
   • 4 lines added
   • CMHomeEmptyStateView.swift

2. CHANGED TYPES
   • CMHomeEmptyStateView (class)
   
3. SYMBOL USAGE ANALYSIS
   CMHomeEmptyStateView:
   • 2 consumer files found
   • CTGoods: ELTHomeEmptyStateViewCell
   • Main App: CTHomeEmptyStateViewCell
   
4. COMPLETE FILE LIST (3 files)
   Direct:
   1. CMHomeEmptyStateView.swift ✏️
   
   Indirect:
   2. ELTHomeEmptyStateViewCell.swift 🔗
   3. CTHomeEmptyStateViewCell.swift 🔗
   
5. RISK ASSESSMENT: LOW
   • No breaking changes
   • 2 files need review
   • Additive changes only
   
6. REVIEW CHECKLIST
   ✅ Git fetch executed
   ✅ Changed files identified
   ✅ Symbol usage analyzed
   ✅ Dependency mapping complete
```

---

**Ready to analyze your PR!** Provide the base and target branches to start comprehensive impact analysis.

---

**This prompt ensures NO file is missed in your PR analysis!** 🎯