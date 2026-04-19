---
description: "Scaffold basic iOS files following MVVM-C architecture patterns"
mode: "agent"
---

# iOS Basic File Scaffolding

Create basic barebone iOS files following our MVVM-C architecture and coding conventions.

## Instructions

Reference our iOS development guidelines:

-   **Primary**: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)
-   **Fallback**: [AI Agent Context](../../AGENTS.md) (if primary unavailable)

Generate basic scaffold files with:

-   Proper MARK sections and imports
-   MVVM+C protocol structure
-   CTDesignSystem components
-   RxSwift patterns
-   TODO comments for implementation

## Required Imports

```swift
import UIKit
import CTDesignSystem
import CTCommon
import CTLocalize
import CTComponent
import CTAsset
import RxSwift
import RxRelay
import Swinject
import CTTracking
import SnapKit
```

## ViewController Template

```swift
import UIKit
import CTDesignSystem
import CTCommon
import CTLocalize
import CTComponent
import CTAsset
import RxSwift
import RxRelay
import Swinject
import CTTracking
import SnapKit

final class [Name]ViewController: UIViewController, [Name]Presentable {

    // MARK: - Properties

    enum Config {
        // TODO: Add configuration constants like sizes, offsets, durations
        // static let standardSize: CGFloat = 44
        // static let padding: CGFloat = 16
    }

    var viewModel: [Name]ViewModelType?
    weak var listener: [Name]PresentableListener?

    // TODO: Add BehaviorRelay and PublishRelay properties based on your needs
    // var isLoadingRelay = BehaviorRelay<Bool>(value: false)
    // var errorMessage = BehaviorRelay<String?>(value: nil)
    // var triggerSomeAction = PublishRelay<Void>()

    let disposeBag = DisposeBag()

    // MARK: - UI Components

    // TODO: Add lazy var UI components using CTDesignSystem
    // Example:
    // private var themeType = ThemeType.default
    // private var theme: CMTheme { DefaultTheme.themeWithType(type: themeType) }
    //
    // lazy var titleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))
    //     label.text = "Hello"
    //     return label
    // }()
    //
    // lazy var subtitleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textPrimary.color))
    //     label.text = "World"
    //     return label
    // }()

    // MARK: - Life Cycle

    override func viewDidLoad() {
        super.viewDidLoad()
        setupViews()
        setupActions()
        configurePresenter()
        configureViewModel()
    }

    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        // TODO: Add viewWillAppear logic
    }

    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        // TODO: Add viewWillDisappear logic
    }

    deinit {
        // TODO: Add cleanup if needed
        NotificationCenter.default.removeObserver(self)
    }

    // MARK: - Private Methods

    private func setupViews() {
        // TODO: Setup UI hierarchy and constraints with SnapKit
        // Example:
        // view.addSubview(someView)
        // someView.snp.makeConstraints { make in
        //     make.edges.equalToSuperview()
        // }
    }

    private func setupActions() {
        // TODO: Setup button targets and gesture recognizers
    }

    private func configurePresenter() {
        // TODO: Bind presenter relays to UI updates
    }

    private func configureViewModel() {
        // TODO: Configure viewModel and call didBecomeActive
    }
}
```

## ViewModel Template

```swift
import RxSwift
import RxRelay
import Action
import CTCommon

// MARK: - ViewModelType
protocol [Name]ViewModelType: CTViewModelType {
    var presenter: [Name]Presentable? { get set }
    var router: [Name]Router? { get set }
    var listener: [Name]PresentableListener? { get set }
}

// MARK: - Presentable
protocol [Name]Presentable: AnyObject {
    var listener: [Name]PresentableListener? { get set }
    // TODO: Add BehaviorRelay and PublishRelay properties based on your UI needs
    // var isLoadingRelay: BehaviorRelay<Bool> { get set }
    // var errorMessage: BehaviorRelay<String?> { get set }
    // var datasource: BehaviorRelay<[SomeModel]> { get set }
    // var triggerSomeAction: PublishRelay<SomeInputType> { get set }
}

// MARK: - PresentableListener
protocol [Name]PresentableListener: AnyObject {
    // TODO: Add PublishRelay properties for triggers from ViewController to ViewModel
    // var triggerSomeAction: PublishRelay<SomeInputType> { get }
    // func handleSomeEvent()
}

// MARK: - Router
protocol [Name]Router: AnyObject {
    // TODO: Add navigation methods
    // func navigateToSomeScreen()
}

final class [Name]ViewModel: [Name]ViewModelType, [Name]PresentableListener {

    // MARK: - Properties

    weak var presenter: [Name]Presentable?
    weak var router: [Name]Router?
    weak var listener: [Name]PresentableListener?

    // TODO: Add UseCase dependencies
    // private let someUseCase: SomeUseCaseType

    let disposeBag = DisposeBag()

    // MARK: - Initialization

    init(
        // TODO: Add UseCase dependencies
        // someUseCase: SomeUseCaseType
    ) {
        // TODO: Initialize dependencies
        // self.someUseCase = someUseCase
    }

    // MARK: - Life Cycle

    func didBecomeActive() {
        presenter?.listener = self
        configureListener()
        configurePresenter()
    }

    // MARK: - Private Methods

    private func configureListener() {
        // TODO: Subscribe to triggers from presenter/UI
        // presenter?.triggerSomeAction.subscribeNext { [weak self] input in
        //     self?.handleSomeAction(input)
        // }.disposed(by: disposeBag)
    }

    private func configurePresenter() {
        // TODO: Subscribe to UseCase responses and update presenter
        // someUseCase.action?.elements
        //     .observe(on: MainScheduler.instance)
        //     .subscribeNext { [weak self] result in
        //         self?.presenter?.datasource.accept(result)
        //     }.disposed(by: disposeBag)
    }

    // MARK: - [Name]PresentableListener

    // TODO: Implement methods from PresentableListener protocol
}
```

## File Types to Generate

Based on the file type requested, generate the appropriate files:

### ViewController (${input:fileName})

-   Create ViewController with proper MARK organization
-   Include proper imports and lifecycle methods
-   Follow naming conventions with "ViewController" suffix
-   Include protocol conformance structure
-   Use CTDesignSystem for UI components

### ViewModel (${input:fileName})

-   Create ViewModel implementing CTViewModelType
-   Include Presenter, PresenterListener, Router, and Listener protocols
-   Use proper RxSwift patterns with BehaviorRelay and PublishRelay
-   Include proper initialization and dependency injection

### UseCase (${input:fileName})

-   Create UseCase following CTUseCaseType or CTActionUseCaseType
-   Include proper Input/Output typealias
-   Implement repository pattern integration
-   Include proper error handling

### Repository (${input:fileName})

-   Create Repository protocol and implementation
-   Include proper service layer integration
-   Follow dependency injection patterns
-   Include proper Observable return types

### Service (${input:fileName})

-   Create Service protocol and implementation
-   Include proper API target integration
-   Follow Requestable pattern for network calls
-   Include proper error handling and mapping

### Model (${input:fileName})

-   Create model with proper property definitions
-   Include Codable conformance when needed
-   Follow proper naming conventions
-   Include proper documentation

### TableViewCell/CollectionViewCell (${input:fileName})

-   Create cell with proper XIB structure
-   Include ViewModel for cell configuration
-   Follow reusable cell patterns
-   Include proper constraint setup
-   Use CTDesignSystem for UI components

## Template Variables

-   `${input:fileName}`: The base name (e.g., "UserProfile")
-   `${input:module}`: The module name (e.g., "CTUserManagement")
-   `${input:fileType}`: The file type (ViewController, ViewModel, UseCase, etc.)

## Output

Generate basic scaffolding with:

1. Required imports including CTDesignSystem
2. Proper MARK sections
3. MVVM+C protocol structure
4. RxSwift patterns with BehaviorRelay/PublishRelay
5. Config enum for constants
6. Lazy var pattern for UI components
7. TODO comments for implementation

Keep implementations minimal with TODO guidance for developers.
