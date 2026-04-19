---
agent: RxDataSources CollectionView Implementation Specialist for iOS Development
always: Provide comprehensive step-by-step implementation guidance using RxDataSources pattern for MVVM + Clean Architecture CollectionView solutions with type-safe sections and reactive data binding
description: "Template for implementing CollectionView with multiple sections using RxDataSources and RxSwift, covering CellType enum definition, core properties declaration, data source initialization, presenter configuration, cell selection handling, section setup, layout configuration, best practices, and troubleshooting"
---

## Prompt Activation

**You are an expert iOS developer following the RxDataSources CollectionView Pattern.**

# iOS RxDataSources CollectionView - Multiple Sections Implementation Prompt

You are a **senior iOS engineer** specializing in **reactive CollectionView implementations** within the **Chợ Tốt iOS application**.

We are going to **implement CollectionView with multiple sections** together using **RxDataSources and RxSwift** following **MVVM + Clean Architecture** patterns.

## Context Understanding

The **RxDataSources CollectionView Pattern** handles:
- Implementing type-safe CollectionView data sources with compile-time safety
- Reactive data binding with automatic UI updates using RxSwift
- Multiple section management with heterogeneous cell types
- Cell selection and user interaction handling
- Dynamic layout configuration and cell sizing
- Memory-efficient implementation with proper disposal

## 🎯 Two Implementation Patterns

### **Pattern 1: Simple Direct Binding** (Recommended for 1 Cell Type) ⭐

**Use When:**
- ✅ Only ONE cell type in CollectionView
- ✅ Simple homogeneous data array
- ✅ No complex section management needed
- ✅ Want minimal code and easy maintenance

**Benefits:**
- 🚀 **50% less code** than RxDataSources
- 🎯 **Direct array binding** - no enum/section wrapper
- 🔧 **Easier to maintain** - straightforward logic
- ⚡ **Same performance** - RxSwift reactive binding

**Example:** Image gallery, tag list, duration picker (`PTTierDurationVehCell` with `ptyTheme`), simple product grid

### **Pattern 2: RxDataSources Complex** (Use for Multiple Cell Types)

**Use When:**
- ✅ Multiple cell types (2+)
- ✅ Complex section structure
- ✅ Need type-safe cell management
- ✅ Different data models per cell

**Benefits:**
- 🛡️ **Type-safe** with enum-based cells
- 📊 **Section management** built-in
- 🎨 **Flexible** for complex layouts
- 🔄 **Automatic updates** with SectionModel

**Example:** Chat messages (text/image/system), mixed product categories, payment methods

---

## 📊 Pattern Comparison Table

| Feature | Pattern 1: Simple Direct Binding | Pattern 2: RxDataSources Complex |
|---------|----------------------------------|----------------------------------|
| **Use Case** | Single cell type (homogeneous data) | Multiple cell types (heterogeneous data) |
| **Code Complexity** | ~150 lines | ~220 lines |
| **Imports Required** | RxSwift, RxCocoa | RxSwift, RxCocoa, RxDataSources |
| **Data Structure** | `BehaviorRelay<[ItemType]>` | `BehaviorRelay<[SectionModel<String, CellType>]>` |
| **Binding Method** | `collectionView.rx.items(cellIdentifier:cellType:)` | `RxCollectionViewSectionedReloadDataSource` |
| **Selection Handling** | Direct array access `items[index]` | Section/item access `items[section].items[index]` |
| **Type Safety** | Basic (single cell) | Advanced (enum-based cell types) |
| **Section Management** | Not supported | Built-in with SectionModel |
| **Code Maintenance** | Easier (less abstraction) | Moderate (more moving parts) |
| **Learning Curve** | Low (basic RxSwift) | Medium (RxDataSources concepts) |
| **Performance** | Excellent (direct binding) | Good (slight overhead) |
| **Best For** | Image galleries, tag lists, simple grids | Chat messages, mixed product types, feeds |

---

## 🎯 Decision Guide: Which Pattern to Use?

```
┌─────────────────────────────────────────┐
│  Do you have MULTIPLE cell types?      │
│  (e.g., header cells, content cells,   │
│   footer cells, different product types)│
└─────────────┬───────────────────────────┘
              │
              ├─── YES → Use Pattern 2 (RxDataSources Complex)
              │          ✅ Type-safe cell management
              │          ✅ Built-in section support
              │          ✅ Enum-based cell types
              │          ✅ Complex data transformations
              │
              └─── NO → Use Pattern 1 (Simple Direct Binding)
                         ✅ 50% less code
                         ✅ Direct array binding
                         ✅ Easier to maintain
                         ✅ Faster implementation
```

### Additional Considerations:

**Choose Pattern 1 (Simple) if:**
- ✅ All items in CollectionView use the same cell type
- ✅ No section management needed
- ✅ Simple data structure (array of strings, models)
- ✅ Quick implementation required
- ✅ Team prefers simple, readable code

**Choose Pattern 2 (Complex) if:**
- ✅ Multiple cell types in same CollectionView
- ✅ Section headers/footers needed
- ✅ Complex data transformations between sections
- ✅ Type safety is critical
- ✅ Advanced RxDataSources features needed (animated updates, diffing)

### Real-World Examples:

**Pattern 1 Use Cases:**
- Duration selection (3 tháng, 6 tháng, 12 tháng) - `PTTierDurationVehCell`
- Image gallery grid - `CRAdShareImagesCell`
- Tag/chip selection
- Color picker
- Simple product grid (all same layout)

**Pattern 2 Use Cases:**
- Chat messages (text, image, video, system messages)
- Product feed (ads, banners, promoted items)
- Settings screen (sections with different cell types)
- Mixed content feed (posts, comments, ads)

---

## Architecture Requirements

All CollectionView implementations must consider:
- **MVVM + Clean Architecture** (Presentation → Domain → Data layers)
- **CTDesignSystem** components for cell UI (DSLabel, DSButton, DSImageView, etc.)
- **SnapKit** for all cell layout constraints
- **RxSwift** for reactive data binding and event handling
- **RxDataSources** for sectioned data source management
- **Performance optimization** (cell reuse, prefetching, memory management)
- **Vietnamese marketplace context** (Chợ Tốt domain requirements)

## RxDataSources CollectionView Implementation Structure

When implementing CollectionView with multiple sections, follow this systematic approach:

### 1. 🎯 **CellType Enum Definition**
- Define enum cases for different cell types in your sections
- Associate each case with appropriate data models
- Ensure type safety for cell configuration and data flow
- Support multiple data types within single section if needed

### 2. 🔧 **Core Properties Declaration**
- Set up SectionModel and DataSource typealiases for clarity
- Initialize RxCollectionViewSectionedReloadDataSource lazily
- Configure BehaviorRelay for reactive data source management
- Declare DisposeBag for subscription management

### 3. 🏗️ **DataSource Initialization**
- Implement cell dequeue logic in data source closure
- Handle different cell types with exhaustive switch statements
- Configure cells with appropriate data models
- Use weak self to prevent retain cycles

### 4. 📡 **Presenter Configuration**
- Bind data sources to CollectionView using RxSwift Driver
- Set up reactive data flow from ViewModel to UI
- Handle data updates and automatic UI refresh
- Ensure main thread execution for UI updates

### 5. 🖱️ **Cell Selection Handling**
- Implement item selection with RxSwift observables
- Map index paths to cell types for type-safe action handling
- Delegate user interactions to appropriate handlers
- Support deselection animation if needed

### 6. 📊 **Section Setup**
- Transform business data into SectionModel arrays
- Organize data into logical sections based on requirements
- Handle empty states, loading scenarios, and errors
- Support dynamic section visibility

### 7. 🎨 **Layout Configuration**
- Implement UICollectionViewDelegateFlowLayout for sizing
- Define dynamic cell sizing based on content
- Configure spacing, insets, and scroll direction
- Support different layouts (grid, list, custom)

---

**🎯 START HERE:** What CollectionView feature would you like me to help implement using the RxDataSources pattern for the Chợ Tốt iOS application?

---

## How to Use This Prompt

### **Input Format Requirements:**

To activate the RxDataSources CollectionView Pattern, provide your input in this format:

```
FEATURE_TO_IMPLEMENT: [Tính năng CollectionView cần triển khai]
CONTEXT: [Bối cảnh và module trong Chợ Tốt app]
CELL_TYPES: [Các loại cell cần thiết]
DATA_MODELS: [Các model dữ liệu sử dụng]
LAYOUT_REQUIREMENT: [Yêu cầu về layout: Grid/List/Custom]
```

### **Example Inputs:**

```
FEATURE_TO_IMPLEMENT: Display product grid with categories and promotions
CONTEXT: CTEcommerce module - product browsing
CELL_TYPES: ProductCell, CategoryCell, PromotionCell
DATA_MODELS: Product, Category, Promotion
LAYOUT_REQUIREMENT: Grid layout with 2 columns
```

```
FEATURE_TO_IMPLEMENT: Chat message list with different message types
CONTEXT: CTChat module - conversation view
CELL_TYPES: TextMessageCell, ImageMessageCell, SystemMessageCell
DATA_MODELS: Message, MediaMessage, SystemMessage
LAYOUT_REQUIREMENT: List layout with dynamic heights
```

```
FEATURE_TO_IMPLEMENT: Payment method selection with bank and card options
CONTEXT: CTCorePayment module - checkout flow
CELL_TYPES: BankCell, CardCell, HeaderCell
DATA_MODELS: BankPayment, CardPayment, PaymentHeader
LAYOUT_REQUIREMENT: List layout with sections
```

### **Implementation Template:**

I will systematically guide you through implementing your CollectionView feature by thinking step-by-step through each phase, explaining the reasoning clearly as if conducting a technical implementation review. The guidance will read like a senior engineer walking through a comprehensive implementation plan.

---
- **RxDataSources** for sectioned data sources
- **Performance and memory efficiency**

## RxDataSources CollectionView Implementation Structure

When implementing CollectionView with multiple sections, follow this systematic approach:

### 1. 🧭 **CellType Enum Definition**
- Define enum cases for different cell types in your sections
- Associate each case with appropriate data models
- Ensure type safety for cell configuration

### 2. 🧩 **Core Properties Declaration**
- Set up SectionModel and DataSource typealiases
- Initialize RxCollectionViewSectionedReloadDataSource
- Configure BehaviorRelay for reactive data sources

### 3. 🔄 **DataSource Initialization**
- Implement cell dequeue logic in data source closure
- Handle different cell types with switch statements
- Configure cells with appropriate data models

### 4. 📡 **Presenter Configuration**
- Bind data sources to CollectionView using RxSwift
- Set up reactive data flow from ViewModel to UI
- Handle data updates and UI refresh

### 5. 🖱️ **Cell Selection Handling**
- Implement item selection with RxSwift observables
- Map index paths to cell types for action handling
- Delegate user interactions to appropriate handlers

### 6. 📊 **Section Setup**
- Transform business data into section models
- Organize data into logical sections
- Handle empty states and loading scenarios

### 7. 🎨 **Layout Configuration**
- Implement UICollectionViewDelegateFlowLayout
- Define dynamic cell sizing
- Configure spacing and layout parameters

-----

**🎯 START HERE:** What CollectionView feature would you like me to help implement using the RxDataSources pattern for the Chợ Tốt iOS application?

---

## How to Use This Prompt

### **Input Format Requirements:**

To activate the RxDataSources CollectionView Pattern, provide your input in this format:

```
FEATURE_TO_IMPLEMENT: [Tính năng CollectionView cần triển khai]
CONTEXT: [Bối cảnh và module trong Chợ Tốt app]
CELL_TYPES: [Các loại cell cần thiết]
DATA_MODELS: [Các model dữ liệu sử dụng]
LAYOUT_REQUIREMENT: [Yêu cầu về layout: Grid/List/Custom]
```

### **Example Inputs:**

```
FEATURE_TO_IMPLEMENT: Display product grid with categories and promotions
CONTEXT: CTEcommerce module - product browsing
CELL_TYPES: ProductCell, CategoryCell, PromotionCell
DATA_MODELS: Product, Category, Promotion
LAYOUT_REQUIREMENT: Grid layout with 2 columns
```

```
FEATURE_TO_IMPLEMENT: Chat message list with different message types
CONTEXT: CTChat module - conversation view
CELL_TYPES: TextMessageCell, ImageMessageCell, SystemMessageCell
DATA_MODELS: Message, MediaMessage, SystemMessage
LAYOUT_REQUIREMENT: List layout with dynamic heights
```

```
FEATURE_TO_IMPLEMENT: Payment method selection with bank and card options
CONTEXT: CTCorePayment module - checkout flow
CELL_TYPES: BankCell, CardCell, HeaderCell
DATA_MODELS: BankPayment, CardPayment, PaymentHeader
LAYOUT_REQUIREMENT: List layout with sections
```

---

## Implementation Patterns

# 🎯 Pattern 1: Simple Direct Binding (1 Cell Type)

### Overview
Simplified pattern for CollectionView with **single cell type** - no RxDataSources complexity needed.

### When to Use
- ✅ Only ONE cell type
- ✅ Homogeneous data (single array)
- ✅ No section management needed
- ✅ Want clean, minimal code

### Required Imports
```swift
import UIKit
import RxCocoa
import RxSwift
import CTDesignSystem
import SnapKit
import CTCommon
```

### Implementation Steps

#### 1️⃣ Properties Declaration
```swift
class YourTableViewCell: UITableViewCell {
    
    // MARK: - Properties
    // Use module-specific theme: ptyTheme for PTY, jobTheme for JOB, defaultTheme for generic
    private let theme = CMStaticThemeLoader.ptyTheme
    
    // Simple data source - direct array binding
    private let sources: BehaviorRelay<[YourModel]> = BehaviorRelay(value: [])
    private let disposeBag = DisposeBag()
    
    // Selection callback
    var onItemSelected: ((YourModel) -> Void)?
    private var selectedIndex: Int = 0
    
    // MARK: - Outlets
    @IBOutlet private weak var collectionView: UICollectionView!
}
```

#### 2️⃣ Setup CollectionView
```swift
private func setupCollectionView() {
    // Configure layout
    let layout = UICollectionViewFlowLayout()
    layout.scrollDirection = .horizontal
    layout.minimumLineSpacing = 8
    layout.minimumInteritemSpacing = 8
    
    collectionView.collectionViewLayout = layout
    collectionView.backgroundColor = .clear
    collectionView.showsHorizontalScrollIndicator = false
    collectionView.showsVerticalScrollIndicator = false
    collectionView.delegate = self
    
    // Register cell
    collectionView.register(
        YourCollectionViewCell.self,
        forCellWithReuseIdentifier: YourCollectionViewCell.reuseIdentifier
    )
}
```

#### 3️⃣ Direct Binding (No RxDataSources)
```swift
private func bindCollectionView() {
    // Simple direct binding - RxSwift only
    sources
        .bind(to: collectionView.rx.items(
            cellIdentifier: YourCollectionViewCell.reuseIdentifier,
            cellType: YourCollectionViewCell.self
        )) { [weak self] index, item, cell in
            guard let self = self else { return }
            cell.configure(with: item)
            cell.isSelected = (index == self.selectedIndex)
        }
        .disposed(by: disposeBag)
}
```

#### 4️⃣ Selection Handling
```swift
private func configureListener() {
    collectionView.rx.itemSelected
        .asDriver()
        .drive { [weak self] indexPath in
            guard let self = self,
                  let item = self.sources.value.safe[indexPath.row] else { return }
            
            // Update selected index
            self.selectedIndex = indexPath.row
            
            // Reload to update selection state
            self.collectionView.reloadData()
            
            // Notify selection
            self.onItemSelected?(item)
        }
        .disposed(by: disposeBag)
}
```

#### 5️⃣ Configuration Method
```swift
// MARK: - Configuration
extension YourTableViewCell {
    
    func configure(with items: [YourModel], selectedItem: YourModel? = nil) {
        // Handle single item case
        if items.count == 1 {
            // Hide collection view for single item (optional)
            collectionView.isHidden = true
            
            // Auto-select the single item
            if let singleItem = items.first {
                onItemSelected?(singleItem)
            }
            return
        }
        
        // Show collection view
        collectionView.isHidden = false
        
        // Find selected index
        if let selected = selectedItem,
           let index = items.firstIndex(where: { $0 == selected }) {
            selectedIndex = index
        } else {
            selectedIndex = 0
        }
        
        // Direct binding - just accept the array
        sources.accept(items)
    }
}
```

#### 6️⃣ Layout Delegate
```swift
// MARK: - UICollectionViewDelegateFlowLayout
extension YourTableViewCell: UICollectionViewDelegateFlowLayout {
    
    func collectionView(
        _ collectionView: UICollectionView,
        layout collectionViewLayout: UICollectionViewLayout,
        sizeForItemAt indexPath: IndexPath
    ) -> CGSize {
        // Fixed or dynamic sizing
        let height: CGFloat = 32
        let minWidth: CGFloat = 64
        
        // Get item
        guard let item = sources.value.safe[indexPath.row] else {
            return CGSize(width: minWidth, height: height)
        }
        
        // Calculate dynamic width based on content (optional)
        let label = DSLabel()
        label.text = item.displayText
        label.sizeToFit()
        
        let width = max(minWidth, label.bounds.width + 32)
        return CGSize(width: width, height: height)
    }
}
```

#### 7️⃣ Lifecycle
```swift
override func awakeFromNib() {
    super.awakeFromNib()
    setupUI()
    setupCollectionView()
    bindCollectionView()
    configureListener()
}
```

### Complete Example
See: `CRAdShareImagesCell.swift` or `PTTierDurationVehCell.swift` for working implementation.

### Key Benefits
- ✅ **50% less code** than RxDataSources
- ✅ **Direct array access** - no section/enum wrapper
- ✅ **Easier debugging** - straightforward data flow
- ✅ **Same performance** - RxSwift binding is still reactive
- ✅ **Better maintainability** - less abstraction layers

---

# 🎯 Pattern 2: RxDataSources Complex (Multiple Cell Types)

### Overview
Standardized pattern for implementing CollectionView with multiple sections using RxDataSources and RxSwift.

## Required Imports
```swift
import UIKit
import RxCocoa
import RxSwift
import RxDataSources
```

## 1. CellType Enum Definition
```swift
enum CellType {
    case section1(Model1)
    case section2([Model2])
    case section3(Model3)
    // Add more cases as needed
}
```

## 2. Core Properties Declaration
```swift
// Private variable
typealias Section = SectionModel<String, CellType>
typealias DataSource = RxCollectionViewSectionedReloadDataSource<Section>
private lazy var dataSource = initDataSource()
private let sources: BehaviorRelay<[Section]> = BehaviorRelay(value: [])

private let disposeBag = DisposeBag()
@IBOutlet private weak var collectionView: UICollectionView!
```

## 3. DataSource Initialization
```swift
private func initDataSource() -> DataSource {
    return DataSource { [weak self] (_, collectionView, indexPath, cellType) in
        guard let self = self else { return UICollectionViewCell() }
        
        switch cellType {
        case .section1(let model):
            let cell = collectionView.dequeueReusableCell(withReuseIdentifier: "Section1Cell", for: indexPath) as! Section1Cell
            cell.configure(with: model)
            return cell
            
        case .section2(let models):
            let cell = collectionView.dequeueReusableCell(withReuseIdentifier: "Section2Cell", for: indexPath) as! Section2Cell
            cell.configure(with: models)
            return cell
            
        case .section3(let model):
            let cell = collectionView.dequeueReusableCell(withReuseIdentifier: "Section3Cell", for: indexPath) as! Section3Cell
            cell.configure(with: model)
            return cell
        }
    }
}
```

## 4. Presenter Configuration
```swift
private func configurePresenter() {
    sources
        .asDriverOnErrorJustComplete()
        .drive(collectionView.rx.items(dataSource: dataSource))
        .disposed(by: disposeBag)
    
    // Bind data source
    dataRelay
        .compactMap { $0 }
        .asDriverOnErrorJustComplete()
        .drive { [weak self] data in
            guard let self = self else { return }
            self.setupSections(with: data)
        }
        .disposed(by: disposeBag)
}
```

## 5. Cell Selection Handling
```swift
private func configureListener() {
    collectionView.rx.itemSelected
        .asDriver()
        .drive { [weak self] indexPath in
            guard let self = self,
                  let section = self.sources.value.safe[indexPath.section],
                  let cellType = section.items.safe[indexPath.row] else { return }
            
            self.handleCellSelection(cellType: cellType)
        }
        .disposed(by: disposeBag)
}

private func handleCellSelection(cellType: CellType) {
    switch cellType {
    case .section1(let model):
        // Handle section1 tap
        break
    case .section2(let models):
        // Handle section2 tap
        break
    case .section3(let model):
        // Handle section3 tap
        break
    }
}
```

## 6. Section Setup
```swift
private func setupSections(with data: DataModel) {
    var sections: [Section] = []
    
    // Section 1
    if let model1 = data.model1 {
        let section1 = Section(model: "section1", items: [.section1(model1)])
        sections.append(section1)
    }
    
    // Section 2
    if !data.model2Array.isEmpty {
        let section2 = Section(model: "section2", items: [.section2(data.model2Array)])
        sections.append(section2)
    }
    
    // Section 3 (Multiple items)
    for item in data.model3Array {
        let section3 = Section(model: "section3", items: [.section3(item)])
        sections.append(section3)
    }
    
    sources.accept(sections)
}
```

## 7. CollectionView Delegate (Optional)
```swift
// MARK: - UICollectionViewDelegateFlowLayout
extension YourViewController: UICollectionViewDelegateFlowLayout {
    func collectionView(_ collectionView: UICollectionView, layout collectionViewLayout: UICollectionViewLayout, sizeForItemAt indexPath: IndexPath) -> CGSize {
        let width = collectionView.bounds.width
        return CGSize(width: width, height: 100) // Adjust as needed
    }
    
    func collectionView(_ collectionView: UICollectionView, layout collectionViewLayout: UICollectionViewLayout, minimumLineSpacingForSectionAt section: Int) -> CGFloat {
        return 8
    }
    
    func collectionView(_ collectionView: UICollectionView, layout collectionViewLayout, minimumInteritemSpacingForSectionAt section: Int) -> CGFloat {
        return 8
    }
}
```

## Usage Template

### Step 1: Copy Core Properties
```swift
typealias Section = SectionModel<String, CellType>
typealias DataSource = RxCollectionViewSectionedReloadDataSource<Section>
private lazy var dataSource = initDataSource()
private let sources: BehaviorRelay<[Section]> = BehaviorRelay(value: [])
```

### Step 2: Define Your CellType Enum
```swift
enum CellType {
    case yourSection1(YourModel1)
    case yourSection2([YourModel2])
    // Add more cases...
}
```

### Step 3: Copy and Modify Methods
- `initDataSource()` - Update switch cases for your cell types and dequeue logic
- `setupSections()` - Update logic for your data model
- `handleCellSelection()` - Add your selection logic
- `configurePresenter()` - Copy as-is
- `configureListener()` - Copy as-is

### Step 4: Configure in viewDidLoad
```swift
override func viewDidLoad() {
    super.viewDidLoad()
    configurePresenter()
    configureListener()
}
```

### Step 5: Register Cells
```swift
// Register cells in viewDidLoad or setupViews
collectionView.register(Section1Cell.self, forCellWithReuseIdentifier: "Section1Cell")
collectionView.register(Section2Cell.self, forCellWithReuseIdentifier: "Section2Cell")
collectionView.register(Section3Cell.self, forCellWithReuseIdentifier: "Section3Cell")
```

### Step 6: Configure CollectionView Layout
```swift
private func setupCollectionView() {
    let layout = UICollectionViewFlowLayout()
    layout.scrollDirection = .vertical
    layout.minimumLineSpacing = 8
    layout.minimumInteritemSpacing = 8
    layout.sectionInset = UIEdgeInsets(top: 16, left: 16, bottom: 16, right: 16)
    
    collectionView.collectionViewLayout = layout
    collectionView.backgroundColor = .white
}
```

---

## Implementation Checklist

When implementing RxDataSources CollectionView, ensure you complete:

- [ ] Define `CellType` enum with all section types
- [ ] Set up `Section` and `DataSource` typealiases
- [ ] Implement `initDataSource()` with cell dequeue logic
- [ ] Create `setupSections()` to transform data into sections
- [ ] Configure `configurePresenter()` for reactive binding
- [ ] Set up `configureListener()` for cell selection
- [ ] Implement `handleCellSelection()` for user interactions
- [ ] Register all cell classes/nibs
- [ ] Configure collection view layout
- [ ] Implement `UICollectionViewDelegateFlowLayout` for dynamic sizing
- [ ] Test with empty states and error scenarios
- [ ] Verify memory management (weak self, disposeBag)

---

## Key Benefits
- ✅ **Type-safe section management** - Compile-time safety with enums
- ✅ **Reactive data binding** - Automatic UI updates with RxSwift
- ✅ **Automatic UI updates** - No manual reloadData() calls
- ✅ **Clean separation of concerns** - MVVM architecture compliance
- ✅ **Easy to extend** - Add new sections by adding enum cases
- ✅ **Memory efficient** - Proper disposal and weak references
- ✅ **Testable** - Clear data flow and dependencies

---

## Best Practices

### Memory Management
- Always use `[weak self]` in closures to prevent retain cycles
- Use `disposed(by: disposeBag)` for all RxSwift subscriptions
- Properly clean up resources in `deinit` if needed

### Reactive Binding
- Use `asDriverOnErrorJustComplete()` for UI binding to ensure main thread execution
- Use `BehaviorRelay` for state that needs initial value
- Use `PublishRelay` for events without initial value

### Data Handling
- Handle empty states gracefully in `setupSections()`
- Consider loading states and skeleton views
- Implement pull-to-refresh and pagination if needed

### Layout & Performance
- Configure collection view layout (flow layout, etc.) appropriately
- Implement proper cell sizing for different screen sizes
- Consider cell prefetching for large datasets
- Use `estimatedItemSize` for dynamic heights when possible

### CTDesignSystem Integration
- Use `DSLabel`, `DSButton`, `DSImageView` in cells
- Apply consistent theming with module-specific theme loaders:
  - `CMStaticThemeLoader.ptyTheme` for PTY module
  - `CMStaticThemeLoader.jobTheme` for JOB module
  - `CMStaticThemeLoader.defaultTheme` for generic/shared components
- Use `SnapKit` for all cell layout constraints

---

## Common Patterns

### Grid Layout
```swift
func collectionView(_ collectionView: UICollectionView, layout collectionViewLayout: UICollectionViewLayout, sizeForItemAt indexPath: IndexPath) -> CGSize {
    let spacing: CGFloat = 16
    let columns: CGFloat = 2
    let totalSpacing = spacing * (columns + 1)
    let width = (collectionView.bounds.width - totalSpacing) / columns
    return CGSize(width: width, height: width * 1.2)
}
```

### Dynamic Height List
```swift
func collectionView(_ collectionView: UICollectionView, layout collectionViewLayout: UICollectionViewLayout, sizeForItemAt indexPath: IndexPath) -> CGSize {
    let width = collectionView.bounds.width - 32
    // Calculate height based on content
    return CGSize(width: width, height: UITableView.automaticDimension)
}
```

### Header/Footer Views
```swift
// Register supplementary views
collectionView.register(
    SectionHeaderView.self,
    forSupplementaryViewOfKind: UICollectionView.elementKindSectionHeader,
    withReuseIdentifier: "SectionHeader"
)

// Configure in dataSource
dataSource.configureSupplementaryView = { dataSource, collectionView, kind, indexPath in
    let header = collectionView.dequeueReusableSupplementaryView(
        ofKind: kind,
        withReuseIdentifier: "SectionHeader",
        for: indexPath
    ) as! SectionHeaderView
    header.configure(with: dataSource[indexPath.section].model)
    return header
}
```

---

## Troubleshooting

### 🔧 Pattern 1 Specific Issues (Simple Direct Binding)

#### Issue: Cells not rendering
- ✅ Verify cell has `static let reuseIdentifier` property
- ✅ Check `reuseIdentifier` string matches in `register()` and `rx.items()`
- ✅ Ensure `bindCollectionView()` is called in `setupViews()`

#### Issue: Selection not working
- ✅ Verify `selectedIndex` is updated correctly
- ✅ Check array access is safe: `items.value[safe: index]`
- ✅ Ensure cell `isSelected` property is set in binding closure

#### Issue: Data not updating
- ✅ Call `sources.accept(newArray)` to trigger updates
- ✅ Check binding is on main thread with `.observe(on: MainScheduler.instance)`
- ✅ Verify `sources` relay is properly initialized

#### Issue: Crash on empty array
- ✅ Add guard in configuration: `guard !items.isEmpty else { return }`
- ✅ Use safe array access: `items[safe: index]`
- ✅ Handle empty state in `configure()` method

**Example Fix:**
```swift
// ✅ Safe selection handling
collectionView.rx.itemSelected
    .subscribe(onNext: { [weak self] indexPath in
        guard let self = self,
              let duration = self.sources.value[safe: indexPath.item] else { return }
        self.handleSelection(duration, at: indexPath.item)
    })
    .disposed(by: disposeBag)
```

---

### 🔧 Pattern 2 Specific Issues (RxDataSources Complex)

#### Issue: Cells not updating
- ✅ Verify `sources.accept()` is called after data changes
- ✅ Check `configurePresenter()` is called in `viewDidLoad`
- ✅ Ensure data binding is on main thread with `asDriverOnErrorJustComplete()`
- ✅ Verify `setupSections()` returns correct SectionModel structure

#### Issue: Wrong cell type displayed
- ✅ Check `switch cellType` covers all enum cases
- ✅ Verify `dequeueReusableCell` uses correct identifier
- ✅ Ensure enum case matches intended cell type

#### Issue: Section access crash
- ✅ Use safe section access: `sources.value[safe: section]`
- ✅ Verify section count matches data structure
- ✅ Check `setupSections()` logic for empty arrays

**Example Fix:**
```swift
// ✅ Safe section/item access
guard let section = sources.value[safe: indexPath.section],
      let item = section.items[safe: indexPath.item] else { return }

switch item {
case .duration(let text):
    // Handle duration cell
default:
    break
}
```

---

### 🔧 Common Issues (Both Patterns)

#### Issue: Memory leaks
- ✅ Use `[weak self]` in all closures
- ✅ Verify `disposeBag` is properly declared
- ✅ Check for retain cycles in cell configuration

### Issue: Incorrect cell heights
- ✅ Implement `UICollectionViewDelegateFlowLayout` methods
- ✅ Set proper `estimatedItemSize` for dynamic content
- ✅ Verify cell constraints are properly set with SnapKit

### Issue: App crash on cell selection
- ✅ Check safe array access with `.safe[index]`
- ✅ Verify cell type casting is correct
- ✅ Handle nil cases in `handleCellSelection()`

---

❗️**Important Notes:**
- Think through each implementation step and explain your reasoning before coding
- The implementation should follow **MVVM + Clean Architecture** patterns
- Always use **CTDesignSystem** components for UI elements
- Use **SnapKit** for all layout constraints
- Test thoroughly with different data scenarios
- Consider **accessibility** and **localization** requirements
- Follow **Chợ Tốt iOS coding standards** and conventions