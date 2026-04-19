# 📚 Complete UseCase Generation Guide for Beginners

**A Step-by-Step Tutorial for iOS Clean Architecture Development**

---

## 🎯 Table of Contents
1. [Prerequisites & Setup](#prerequisites--setup)
2. [Understanding Clean Architecture](#understanding-clean-architecture)
3. [Project Structure Overview](#project-structure-overview)
4. [Step-by-Step UseCase Implementation](#step-by-step-usecase-implementation)

---

## 📋 Prerequisites & Setup

### Step 1: Install Required Software

#### 1.1 Install Visual Studio Code
1. **Download VS Code:**
   - Go to [https://code.visualstudio.com/](https://code.visualstudio.com/)
   - Click "Download for macOS"
   - Open the downloaded `.dmg` file
   - Drag Visual Studio Code to Applications folder

2. **Install Essential Extensions:**
   ```bash
   # Open VS Code and press Cmd+Shift+P, then type "Extensions"
   # Install these extensions:
   - Swift (by Swift Server Work Group)
   - GitHub Copilot (by GitHub)
   - GitLens (by GitKraken)
   - Markdown All in One (by Yu Zhang)
   ```

#### 1.2 Install Xcode
1. **From App Store:**
   - Open Mac App Store
   - Search "Xcode"
   - Click "Get" or "Install"
   - Wait for download (this takes time, it's ~10GB)

2. **Command Line Tools:**
   ```bash
   # Open Terminal and run:
   xcode-select --install
   ```

#### 1.3 Install GitHub Copilot
1. **Setup GitHub Copilot:**
   - Open VS Code
   - Press `Cmd+Shift+P`
   - Type "GitHub Copilot: Sign In"
   - Follow the authentication process
   - Verify installation by typing a comment in Swift file

### Step 2: Clone the Project
```bash
# Open Terminal and navigate to your desired folder
cd ~/Desktop
git clone [your-project-url]
cd ct-ios-app--v3
```

### Step 3: Open Project in Tools
```bash
# Open in VS Code
code .

# Open iOS project in Xcode
open CTiOS.xcworkspace
```

---

## 🤖 AI UseCase Generation Prompt

**Copy and paste this prompt to any AI assistant (GitHub Copilot, ChatGPT, Claude) to generate complete UseCase implementation:**

```
Generate and implement a complete UseCase following CTCorePayment 6-layer Clean Architecture:

UseCase: FetchDongtotProfile
Input: String
Output: String 
Endpoint: "v1/dongtot/profile"
Method: get

Implement all 6 layers by adding code directly to project files:

1. Add Api.FetchDongtotProfile = "v1/dongtot/profile" to CRNetworkHelper.swift
2. Add FetchDongtotProfileTarget struct to CRCheckoutTargets.swift
3. Add FetchDongtotProfile method to CRCheckoutService.swift (protocol + implementation)
4. Add FetchDongtotProfile method to CRCheckoutCartRepository.swift (protocol + implementation)
5. Add CRFetchDongtotProfileUseCase class to CRCheckoutUseCase.swift
6. Add executeFetchDongtotProfile method to CRCheckoutPageViewModel.swift (with RxSwift bindings + error handling)
```

**Customize the prompt by changing:**
- UseCase name (e.g., FetchUserData, SaveSettings)
- Input/Output types (e.g., UserID, CustomModel)
- Endpoint URL and HTTP method
- Add your specific business logic requirements

**Usage:**
1. Copy the prompt above
2. Customize for your specific UseCase
3. Paste into your AI assistant
4. Review and implement the generated code

---

## 🏗️ Understanding Clean Architecture

### What is Clean Architecture?
Clean Architecture is a software design pattern that separates code into layers, making it:
- **Testable** - Each layer can be tested independently
- **Maintainable** - Changes in one layer don't affect others
- **Scalable** - Easy to add new features
- **Readable** - Clear separation of concerns

### The 6 Layers in Our iOS App:

```
┌─────────────────────────────────────┐
│         6. ViewModel                │ ← Handles UI logic and user interactions
├─────────────────────────────────────┤
│         5. UseCase                  │ ← Contains business logic and rules
├─────────────────────────────────────┤
│         4. Repository               │ ← Coordinates data from different sources
├─────────────────────────────────────┤
│         3. Service                  │ ← Handles network requests and responses
├─────────────────────────────────────┤
│         2. Targets                  │ ← Defines specific API endpoints
├─────────────────────────────────────┤
│         1. NetworkHelper            │ ← Contains API endpoint constants
└─────────────────────────────────────┘
```

### Real-World Analogy:
Think of ordering food from a restaurant app:

1. **NetworkHelper** = Restaurant menu with all dish names
2. **Targets** = Specific order details (dish name, quantity, special requests)
3. **Service** = Waiter who takes your order to the kitchen
4. **Repository** = Kitchen manager who coordinates between different stations
5. **UseCase** = Chef who applies cooking rules and recipes
6. **ViewModel** = The app interface that shows you order status and handles your taps

---

## 📁 Project Structure Overview

### Understanding the File Structure
```
ct-ios-app--v3/
└── AppFeatures/
    └── CTCorePayment/
        └── CTCorePayment/
            ├── NetworkHelper/
            │   ├── CRNetworkHelper.swift          ← Layer 1: API endpoints
            │   └── ...
            ├── Data/
            │   ├── Services/
            │   │   └── Checkout/
            │   │       ├── CRCheckoutTargets.swift ← Layer 2: API targets
            │   │       └── CRCheckoutService.swift ← Layer 3: Service methods
            │   └── Repositories/
            │       └── Checkout/
            │           └── Cart/
            │               └── CRCheckoutCartRepository.swift ← Layer 4: Repository
            ├── Domain/
            │   └── UseCases/
            │       └── Checkout/
            │           └── CRCheckoutUseCase.swift ← Layer 5: Business logic
            └── Features/
                └── CheckoutPage/
                    └── CRCheckoutPageViewModel.swift ← Layer 6: Presentation logic
```

### File Naming Conventions:
- **CR** = CorePayment module prefix
- **Checkout** = Feature name
- **Service/Repository/UseCase** = Layer type
- **ViewModel** = Presentation layer

---

## 🛠️ Step-by-Step UseCase Implementation

Let's implement a **FetchAdProfile** UseCase step by step!

### 📝 Our Implementation Plan:
- **UseCase:** FetchAdProfile
- **Input:** String (ad identifier)
- **Output:** AdProfile (ad data)
- **Endpoint:** "v1/ads/profile"
- **Method:** GET

---

### 🔥 Layer 1: NetworkHelper (API Constants)

#### What it does:
Stores all API endpoint URLs in one place for easy management.

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRNetworkHelper.swift
   ```

2. **Find the Api extension:**
   ```swift
   extension Api {
       // You'll see existing endpoints like:
       static let fetchCopilot = "v1/private/ai/fetch-copilot"
   }
   ```

3. **Add your new endpoint:**
   ```swift
   extension Api {
       // Existing endpoints...
       static let fetchCopilot = "v1/private/ai/fetch-copilot"
       
       // 🆕 Add this line:
       static let FetchAdProfile = "v1/ads/profile"
   }
   ```

#### ✅ Verification:
- Save the file (`Cmd+S`)
- No compilation errors should appear
- Your endpoint is now available throughout the app

---

### 🎯 Layer 2: Targets (API Request Configuration)

#### What it does:
Defines how to make specific API requests (method, parameters, response handling).

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRCheckoutTargets.swift
   ```

2. **Find the CRCheckoutTargets enum:**
   ```swift
   enum CRCheckoutTargets {
       // You'll see existing targets
   }
   ```

3. **Add your new target:**
   ```swift
   enum CRCheckoutTargets {
       // Existing targets...
       
       // 🆕 Add this struct:
       struct FetchAdProfileTarget: Requestable {
           typealias Output = CRAdProfileResponseModel?
           
           var httpMethod: HTTPMethod { return .get }
           var endpoint: String { return Api.FetchAdProfile }
           var parameterEncoding: ParameterEncoding { return URLEncoding.default }
           
           let input: String
           
           var parameters: [String: Any]? {
               return ["data": input, "timestamp": Date().timeIntervalSince1970]
           }
           
           func decode(data: Any) -> Output {
               guard let data = data as? [String: Any] else { return nil }
               return CRAdProfileResponseModel(JSON: data)
           }
       }
   }
   ```

#### 🧠 Understanding the Code:
- **`typealias Output`** = What type of data this API returns
- **`httpMethod`** = GET, POST, PUT, DELETE
- **`endpoint`** = Uses our API constant from Layer 1
- **`parameterEncoding`** = How to format the request parameters
- **`input`** = The data we send to the API
- **`parameters`** = The actual data packet sent to server
- **`decode`** = Converts server response to Swift objects

#### ✅ Verification:
- Save the file
- Build project (`Cmd+B` in Xcode) - should compile without errors

---

### ⚙️ Layer 3: Service (Network Communication)

#### What it does:
Makes the actual network calls and handles responses.

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRCheckoutService.swift
   ```

2. **Add method to protocol:**
   ```swift
   protocol CRCheckoutServiceType {
       // Existing methods...
       func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?>
       
       // 🆕 Add this line:
       func FetchAdProfile(input: String) -> Observable<CRAdProfileResponseModel?>
   }
   ```

3. **Add implementation:**
   ```swift
   extension CRCheckoutService: CRCheckoutServiceType {
       // Existing implementations...
       
       // 🆕 Add this method:
       func FetchAdProfile(input: String) -> Observable<CRAdProfileResponseModel?> {
           return CRCheckoutTargets.FetchAdProfileTarget(input: input)
               .execute()
               .observe(on: resultScheduler)
       }
   }
   ```

#### 🧠 Understanding the Code:
- **`Observable<CRAdProfileResponseModel?>`** = Returns data asynchronously using RxSwift
- **`FetchAdProfileTarget(input: input)`** = Creates our API request from Layer 2
- **`.execute()`** = Actually makes the network call
- **`.observe(on: resultScheduler)`** = Ensures response comes back on the correct thread

#### ✅ Verification:
- Save the file
- No errors should appear
- The method is now available for repositories to use

---

### 🗄️ Layer 4: Repository (Data Coordination)

#### What it does:
Coordinates data access - could combine network, database, cache, etc.

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRCheckoutCartRepository.swift
   ```

2. **Add method to protocol:**
   ```swift
   protocol CRCheckoutCartRepositoryType {
       // Existing methods...
       func fetchCopilot(input: String) -> Observable<CRCopilotResponseModel?>
       
       // 🆕 Add this line:
       func FetchAdProfile(input: String) -> Observable<CRAdProfileResponseModel?>
   }
   ```

3. **Add implementation:**
   ```swift
   extension CRCheckoutCartRepository: CRCheckoutCartRepositoryType {
       // Existing implementations...
       
       // 🆕 Add this method:
       func FetchAdProfile(input: String) -> Observable<CRAdProfileResponseModel?> {
           return service.FetchAdProfile(input: input)
       }
   }
   ```

#### 🧠 Understanding the Code:
- **Repository pattern** = Abstracts where data comes from
- **`service.FetchAdProfile`** = Delegates to our service from Layer 3
- In the future, we could add caching, database storage, etc. here

#### ✅ Verification:
- Save the file
- Repository now provides clean interface for business logic

---

### 🧠 Layer 5: UseCase (Business Logic)

#### What it does:
Contains the business rules and logic for specific user actions.

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRCheckoutUseCase.swift
   ```

2. **Add your UseCase class:**
   ```swift
   // 🆕 Add this complete class:
   final class CRFetchAdProfileUseCase: CTActionUseCaseType {
       typealias Output = CRAdProfileResponseModel?
       typealias Input = String
       
       let repository: CRCheckoutCartRepositoryType
       var action: Action<Input, Output>?
       
       init(repository: CRCheckoutCartRepositoryType) {
           self.repository = repository
           self.action = initAction()
       }
       
       private func initAction() -> Action<Input, Output> {
           Action<Input, Output> { [unowned self] input in
               self.repository.FetchAdProfile(input: input)
           }
       }
   }
   ```

#### 🧠 Understanding the Code:
- **`CTActionUseCaseType`** = Our app's base UseCase protocol
- **`Action<Input, Output>`** = RxSwift pattern for handling async operations
- **`[unowned self]`** = Memory management to prevent retain cycles
- **`repository.FetchAdProfile`** = Uses our repository from Layer 4

#### ✅ Verification:
- Save the file
- UseCase is now ready to be called by ViewModels

---

### 🖥️ Layer 6: ViewModel (Presentation Logic)

#### What it does:
Handles UI state, user interactions, and coordinates with UseCases.

#### Implementation:
1. **Open the file:**
   ```bash
   # In VS Code, press Cmd+P and type:
   CRCheckoutPageViewModel.swift
   ```

2. **Add the main execution method:**
   ```swift
   // 🆕 Add this method to CRCheckoutPageViewModel class:
   func executeFetchAdProfile(input: String) {
       let useCase = CRFetchAdProfileUseCase(repository: checkoutRepo)
       
       // Handle success
       useCase.action?.elements
           .bind(onNext: { [weak self] result in
               guard let self = self, let result = result else { return }
               self.handleFetchAdProfileSuccess(result)
           })
           .disposed(by: disposeBag)
       
       // Handle loading
       useCase.action?.executing
           .bind(onNext: { [weak self] loading in
               self?.presenter?.loading.accept(loading)
           })
           .disposed(by: disposeBag)
       
       // Handle errors
       useCase.action?.underlyingError
           .bind(onNext: { [weak self] error in
               self?.handleFetchAdProfileError(error)
           })
           .disposed(by: disposeBag)
       
       // Execute
       useCase.action?.execute(input)
   }
   ```

3. **Add helper methods:**
   ```swift
   // 🆕 Add these helper methods:
   private func handleFetchAdProfileSuccess(_ response: CRAdProfileResponseModel) {
       // Handle success response
       guard let adProfile = response.data else { return }
       // Process AdProfile data
       print("FetchAdProfile success: \(adProfile)")
       // Update UI here
   }
   
   private func handleFetchAdProfileError(_ error: Error) {
       presenter?.loading.accept(false)
       // Handle error
       print("FetchAdProfile error: \(error.localizedDescription)")
       // Show error message to user
   }
   ```

#### 🧠 Understanding the Code:
- **`.bind(onNext:)`** = RxSwift way to handle async responses
- **`[weak self]`** = Prevents memory leaks by avoiding strong reference cycles
- **`.disposed(by: disposeBag)`** = Automatic cleanup when ViewModel is destroyed
- **`useCase.action?.execute(input)`** = Triggers the entire chain from Layer 5 → 1

#### ✅ Verification:
- Save the file
- ViewModel can now trigger the complete UseCase flow

---

### 📦 Data Models (Response Handling)

#### What it does:
Defines the data structures for API responses.

#### Implementation:

1. **Create CRAdProfileResponseModel:**
   ```swift
   // 🆕 Create new file: CRAdProfileResponseModel.swift
   import Foundation
   import ObjectMapper

   struct CRAdProfileResponseModel: Mappable {
       var success: Bool?
       var data: AdProfile?
       var message: String?
       var timestamp: TimeInterval?
       
       init?(map: Map) {}
       
       mutating func mapping(map: Map) {
           success <- map["success"]
           data <- map["data"]
           message <- map["message"]
           timestamp <- map["timestamp"]
       }
   }
   ```

2. **Create AdProfile model:**
   ```swift
   // 🆕 Create new file: AdProfile.swift
   import Foundation
   import ObjectMapper

   struct AdProfile: Mappable {
       var adId: String?
       var title: String?
       var description: String?
       var imageUrl: String?
       var targetUrl: String?
       var category: String?
       var priority: Int?
       var isActive: Bool?
       var createdAt: String?
       var updatedAt: String?
       
       init?(map: Map) {}
       
       mutating func mapping(map: Map) {
           adId <- map["ad_id"]
           title <- map["title"]
           description <- map["description"]
           imageUrl <- map["image_url"]
           targetUrl <- map["target_url"]
           category <- map["category"]
           priority <- map["priority"]
           isActive <- map["is_active"]
           createdAt <- map["created_at"]
           updatedAt <- map["updated_at"]
       }
   }
   ```

#### 🧠 Understanding the Code:
- **`Mappable`** = ObjectMapper protocol for JSON ↔ Swift object conversion
- **`init?(map: Map)`** = Required initializer for Mappable
- **`<-`** = ObjectMapper operator to map JSON keys to Swift properties
- **`map["json_key"]`** = Maps JSON key to Swift property


---

## 🎓 Understanding the Flow

### Complete Data Flow Visualization:
```
User taps button
        ↓
ViewModel.executeFetchAdProfile("ad123")
        ↓
UseCase.action.execute("ad123")
        ↓
Repository.FetchAdProfile("ad123")
        ↓
Service.FetchAdProfile("ad123")
        ↓
Target.execute() → HTTP GET to "v1/ads/profile"
        ↓
Server responds with JSON
        ↓
Target.decode() → CRAdProfileResponseModel
        ↓
Service returns Observable<CRAdProfileResponseModel?>
        ↓
Repository returns Observable<CRAdProfileResponseModel?>
        ↓
UseCase returns Observable<CRAdProfileResponseModel?>
        ↓
ViewModel handles success/error/loading
        ↓
UI updates with ad profile data
```