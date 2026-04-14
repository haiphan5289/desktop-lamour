# [AI] Deeplink Implementation Template

## 🎯 **Auto-Execute Deeplink Implementation Workflow**

### **🤖 AI TRIGGER PATTERNS:**
When you type any of these patterns, AI will automatically execute the full workflow:

```
[AI] Deeplink {WorkGroup}: 
1. Work Groups: {workgroup_name}
2. Deeplink: chotot-app://www.chotot.org/{path}
3. Feature Name: {featureName}
4. Target Screen: {targetScreen}  
5. Navigation Method: {navigationMethod}
OR

Deeplink {workgroup_name}: {path} -> {featureName} -> {targetScreen}

OR

Create deeplink for {path} in {workgroup_name} to {targetScreen}
```

### **Input Information Required:**
1. **Work Groups**: `{workgroup_name}` (revenue | ecommerce | goods | chat | etc.)
2. **Deeplink URL**: `chotot-app://www.chotot.org/{path}`
3. **Feature Name**: `{featureName}`
4. **Target Screen**: `{targetScreen}`
5. **Navigation Method**: `{navigationMethod}` (actual implementation - PosManager.shared.openPos..., CTRewardManager.shared.startModule...)
6. **Parameters**: `{parameters}` (optional)

**🔴 CRITICAL: Navigation Method Logic Requirements:**
- ✅ ALL business logic MUST be implemented in dedicated `goto{FeatureName}()` functions
- ✅ Handler switch cases MUST only call the navigation method functions
- ✅ Navigation method functions MUST contain parameter validation, login checks, and actual navigation calls
- ✅ NO direct navigation logic should be in handler switch cases

---

### **🚀 AUTO-EXECUTION WORKFLOW:**

**When AI detects deeplink request patterns above, AI will AUTOMATICALLY:**

1. **🔍 Execute Step 0: AI Processing Log** (MANDATORY FILE SCANNING)
2. **📝 Execute Steps 1-5: Implementation** (Based on workgroup and navigation method)
3. **✅ Provide Implementation Summary** (Show what was modified)

**NO NEED TO ASK - AI WILL JUST DO IT!**

---

### **Implementation Steps:**

#### **🔍 Step 0: AI Processing Log**

## 🔴 CRITICAL AI EXECUTION RULES

**MANDATORY FILE LOGGING:**
- ✅ AI MUST read and log Parser file content immediately (Step 0.1)
- ✅ AI MUST read and log Handler file content (Step 0.2)
- ✅ AI MUST read and log Navigator folder structure (Step 0.3)
- ✅ AI MUST display existing enums, functions, and patterns clearly
- ✅ AI MUST analyze and report conflict detection results
- ✅ AI MUST show all file paths and line numbers for modifications

**MANDATORY DEEPLINK FILE READING:**
- ✅ AI MUST read "{WorkGroup}DeeplinkParser.cs" before any generation
- ✅ AI MUST read "{WorkGroup}DeeplinkHandler.cs" before any generation
- ✅ AI MUST scan "Features/CTCorePayment/CTCorePayment/PaymentManager/" (revenue only)
- ✅ AI MUST scan "Features/CorePayment/PaymentManager/" (revenue only)
- ✅ AI MUST list_dir and read ALL Navigator files with read_file tool
- ✅ AI MUST search for existing navigation functions with grep_search
- ✅ AI MUST analyze existing patterns and avoid duplicates
- ✅ AI MUST NOT skip file analysis under any circumstances
- ✅ AI MUST show file content BEFORE making any modifications
- ✅ AI MUST verify navigation methods exist in Navigator files

**POS FEATURE MANDATORY READING:**
- ✅ If feature is POS, AI MUST read both files before any generation:
    - `ChoTot/Features/Pos/PosManager.cs` ([link](ChoTot/Features/Pos/PosManager.cs))
    - `Features/CTPos/CTPos/POSModule.cs` ([link](Features/CTPos/CTPos/POSModule.cs))

**POST-IMPLEMENTATION VERIFICATION:**
- ✅ AI MUST verify all changes were applied correctly by reading modified files
- ✅ AI MUST provide exact line numbers for all modifications
- ✅ AI MUST show before/after code snippets for critical changes
- ✅ AI MUST validate that enum cases, parser logic, and handler logic are consistent



#### **Step 1: Add Path Enum Case** 
📁 File: 
- If `workgroup_name == revenue` => `RevenueDeeplinkParser.cs`
- If `workgroup_name == ecommerce` => `EcommerceDeeplinkParser.cs`
- If `workgroup_name == goods` => `GoodsDeeplinkParser.cs`
```swift
// In Path{WorkGroup}DeepLinkType enum (around line 25-55)
case {featureName} = "/{path}"
```

#### **Step 2: Add Deeplink Type Enum Case**
📁 File: Same as Step 1
```swift
// In {WorkGroup}DeeplinkType enum (around line 57-85)
case goto{FeatureName}({parameters})
```

#### **Step 3: Add Parser Logic**
📁 File: Same as Step 1
```swift
// In parse() method (around line 110-250) or parseRewardDeeplink() for reward features
if deeplinkModel.url.absoluteString.contains("\(Path{WorkGroup}DeepLinkType.{featureName}.rawValue)") {
    return .{workgroup_name}({WorkGroup}DeeplinkType.goto{FeatureName}({extractedParams}))
}

// OR for reward features, add in parseRewardDeeplink() switch statement:
case .{featureName}:
    return .{workgroup_name}({WorkGroup}DeeplinkType.goto{FeatureName})
```

#### **Step 4: Add Handler Logic**
📁 File: 
- If `workgroup_name == revenue` => `RevenueDeeplinkHandler.cs`
- If `workgroup_name == ecommerce` => `EcommerceDeeplinkHandler.cs`
- If `workgroup_name == goods` => `GoodsDeeplinkHandler.cs`

**🔴 CRITICAL RULE: ALL LOGIC MUST BE PUT INTO NAVIGATION METHOD FUNCTIONS**

```swift
// In handleDeeplink() switch statement (around line 25-95)
case .goto{FeatureName}({parameters}):
    goto{FeatureName}({parameters}) // MUST call navigation method function

// OR for reward features, add in handleRewardDeeplink() switch statement:
case .goto{FeatureName}:
    goto{FeatureName}() // MUST call navigation method function
```

#### **Step 5: Create Navigation Method Function (MANDATORY)**
📁 File: Same as Step 4

**🔴 IMPORTANT: Always create a dedicated navigation method function for the logic**

```swift
// MANDATORY: Add navigation method function in extension
private func goto{FeatureName}({parameters}) {
    // ✅ ALL BUSINESS LOGIC GOES HERE
    // ✅ Parameter validation, login checks, navigation calls, etc.
    
    // Example implementation patterns:
    loginIfNeeded { [weak self] in
        // Navigation logic here
        {actualNavigationMethod}
    }
}
```

---

### **Common Navigation Patterns:**

### **🏆 POS Module Navigation:**
```swift
// For Premium Features
PosManager.shared.openPosPremiumFeatures(cateId: cateId, adId: adId, sourceType: .{sourceType})

// For Listing Fee
PosManager.shared.openPosListingFee(cateId: cateId, adId: adId, sourceType: .{sourceType})

```

#### **🏆 Reward Module Navigation:**
```swift
CTRewardManager.shared.startModule(on: navigationController, with: .{targetType}({params}))
```

#### **💰 Revenue Module Navigation:**
```swift
AccountManager.shared.navigateTo{Feature}({params})
```

#### **🛒 Commerce Navigation:**
```swift
CTPrivateDashboardManager.shared.{method}({params})
```

#### **📱 Tab Navigation:**
```swift
MainTabViewController.shared.navigateTo{Feature}({params})
```

#### **🆕 Navigation Method Function Pattern:**
```swift
// ✅ MANDATORY PATTERN: All logic goes into dedicated navigation method functions
private func goto{FeatureName}({parameters}) {
    // ✅ Parameter validation
    // ✅ Login checks with loginIfNeeded
    // ✅ Actual navigation method calls
    // ✅ Error handling
    
    loginIfNeeded { [weak self] in
        // Actual navigation implementation here
    }
}
```

#### **🔄 Handler Switch Case Pattern:**
```swift
// ✅ Handler switch cases MUST only call navigation method functions
case .goto{FeatureName}({parameters}):
    goto{FeatureName}({parameters}) // Simple function call only
```

---

### **🏗️ Workgroup File Mapping:**

| Workgroup | Parser File | Handler File | Navigator Pattern | Example Path |
|-----------|------------|--------------|------------------|--------------|
| `revenue` | `RevenueDeeplinkParser.cs` | `RevenueDeeplinkHandler.cs` | `CRNavigator+Extension.cs` | `/uu-dai/*`, `/revenue/*` |
| `ecommerce` | `EcommerceDeeplinkParser.cs` | `EcommerceDeeplinkHandler.cs` | - | `/mua-ban/*`, `/shop/*` |
| `goods` | `GoodsDeeplinkParser.cs` | `GoodsDeeplinkHandler.cs` | - | `/tin-dang/*`, `/ad/*` |
| `chat` | `ChatDeeplinkParser.cs` | `ChatDeeplinkHandler.cs` | - | `/chat/*`, `/message/*` |

### **Example Usage:**

#### **Revenue Workgroup Example:**
```markdown
[AI] Deeplink Revenue: 
1. Work Groups: revenue
2. Deeplink: chotot-app://www.chotot.org/uu-dai/diem-tot/da-dung
3. Feature Name: rewardUsed
4. Target Screen: MyDiemTot screen
5. Navigation Method: CTRewardManager.shared.startModule(with: .myDiemTot(type: "used"))
```

#### **Ecommerce Workgroup Example:**
```markdown
[AI] Deeplink Ecommerce: 
1. Work Groups: ecommerce
2. Deeplink: chotot-app://www.chotot.org/shop/products
3. Feature Name: shopProducts
4. Target Screen: Products listing screen
5. Navigation Method: ShopManager.shared.navigateToProducts()
```

#### **POS Feature Example:**
```markdown
[AI] Deeplink Revenue: 
1. Work Groups: revenue
2. Deeplink: chotot-app://www.chotot.org/pos?cateId=7010&adId=165712553&sourceType=bumpDashboard
3. Feature Name: POS
4. Target Screen: POS Premium Features screen
5. Navigation Method: PosManager.shared.openPosPremiumFeatures()
```

#### **New Function Example:**
```markdown
[AI] Deeplink Revenue: 
1. Work Groups: revenue
2. Deeplink: chotot-app://www.chotot.org/pos/checkout
3. Feature Name: posCheckout
4. Target Screen: POS Checkout screen
5. Navigation Method: PosManager.shared.openPosCheckout(type: "stickAd")
6. Parameters: type = "stickAd"
```

**Generated Navigation Method Function:**
```swift
private func gotoPosCheckout(type: String?) {
    let checkoutType = type ?? "default"
    loginIfNeeded { [weak self] in
        PosManager.shared.openPosCheckout(type: checkoutType)
    }
}
```

**Handler Implementation:**
```swift
case .gotoPosCheckout(let type):
    gotoPosCheckout(type: type) // Only calls the navigation method
```

---

### **🔍 Implementation Checklist:**

- [ ] Add path enum case in `Path{WorkGroup}DeepLinkType`
- [ ] Add deeplink type enum case in `{WorkGroup}DeeplinkType`  
- [ ] Add parser logic in appropriate method
- [ ] **🔴 CRITICAL**: Create dedicated navigation method function `goto{FeatureName}()` with ALL business logic
- [ ] Add handler logic in switch case that ONLY calls the navigation method function
- [ ] Test deeplink functionality
- [ ] Verify navigation works correctly
- [ ] Check login requirements if needed
- [ ] **🔴 VERIFY**: Handler switch cases contain NO business logic, only function calls

---

### **📋 Common Path Patterns:**

| Category | Pattern | Example |
|----------|---------|---------|
| Reward | `/uu-dai/{feature}` | `/uu-dai/diem-tot/da-nhan` |
| Transaction | `/lich-su-giao-dich/{type}` | `/lich-su-giao-dich/hop-dong-dong-tot` |
| Revenue | `/revenue/{feature}` | `/revenue/private_dashboard` |
| Payment | `/thanh-toan/{type}` | `/thanh-toan/gio-hang` |
| Package | `/goi-pro/{type}` | `/goi-pro/premium` |
| POS | `/pos` | `/pos?cateId=7010&adId=123&sourceType=bumpDashboard` |

---

### **⚠️ Important Notes:**

1. **Path Format**: Always include leading `/` in enum case
2. **Naming Convention**: Use camelCase for enum cases
3. **Parameter Extraction**: Handle URL parameters properly
4. **Login Check**: Add `loginIfNeeded` for authenticated features
5. **Navigation Context**: Use appropriate navigation controller
6. **Error Handling**: Return `.unknown` for unhandled cases
7. **🔴 CRITICAL - Navigation Method Functions**: 
   - ALL logic MUST be implemented in dedicated `goto{FeatureName}()` functions
   - Handler switch cases MUST only call these navigation method functions
   - Navigation method functions MUST contain ALL business logic, parameter validation, login checks, and navigation calls
8. **Function Naming**: Follow pattern `goto{FeatureName}()` with proper parameters
9. **Parameter Validation**: Always provide default values for extracted parameters to avoid crashes
10. **Source Type Mapping**: For POS features, map string sourceType to appropriate enum values
11. **Login Protection**: Always wrap navigation in `loginIfNeeded` for authenticated features
12. **🔴 NO DIRECT LOGIC IN HANDLERS**: Never put business logic directly in handler switch cases

---

### **🧪 Testing Commands:**

```bash
# Test deeplink in simulator
xcrun simctl openurl booted "chotot-app://www.chotot.org/{path}"

# Test with parameters  
xcrun simctl openurl booted "chotot-app://www.chotot.org/{path}?param1=value1&param2=value2"
```
