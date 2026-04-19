---
description: "Generate basic MVVM-C module structure"
mode: "agent"
---

# iOS Basic Module Generator

Generate basic MVVM-C module with barebone structure following production patterns.

## Instructions

Reference our iOS development guidelines:

-   **Primary**: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)
-   **Fallback**: [AI Agent Context](../../AGENTS.md) (if primary unavailable)

Generate complete module structure with:

-   ViewController implementing Presentable protocol
-   ViewModel implementing ViewModelType and PresentableListener
-   Proper protocol definitions
-   CTDesignSystem usage
-   RxSwift patterns

## Template Variables

-   `${input:moduleName}`: Module name (e.g., "UserProfile")
-   `${input:featureName}`: Feature name (e.g., "CTUserManagement")

## Output Files

1. **[ModuleName]ViewController.swift** - UI layer with CTDesignSystem
2. **[ModuleName]ViewModel.swift** - Business logic with UseCase dependencies
3. **[ModuleName]Builder.swift** - Dependency injection setup

Each file follows production patterns with:

-   Required imports (CTDesignSystem, CTCommon, etc.)
-   Config enum for constants
-   Proper protocol structure
-   RxSwift patterns
-   TODO comments for implementation

## Generated Structure

### ViewController Structure

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
import CTAsset
import RxSwift
import RxRelay
import Swinject
import CTTracking

final class [ModuleName]ViewController: UIViewController, [ModuleName]Presentable {

    // MARK: - Properties

    enum Config {
        // TODO: Add configuration constants
        // static let standardSize: CGFloat = 44
        // static let padding: CGFloat = 16
    }

    var viewModel: [ModuleName]ViewModelType?
    weak var listener: [ModuleName]PresentableListener?

    // TODO: Add BehaviorRelay and PublishRelay properties
    // var isLoadingRelay = BehaviorRelay<Bool>(value: false)
    // var errorMessage = BehaviorRelay<String?>(value: nil)

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

### ViewModel Structure

```swift
import RxSwift
import RxRelay
import Action
import CTCommon

// MARK: - ViewModelType
protocol [ModuleName]ViewModelType: CTViewModelType {
    var presenter: [ModuleName]Presentable? { get set }
    var router: [ModuleName]Router? { get set }
    var listener: [ModuleName]PresentableListener? { get set }
}

// MARK: - Presentable
protocol [ModuleName]Presentable: AnyObject {
    var listener: [ModuleName]PresentableListener? { get set }
    // TODO: Add BehaviorRelay and PublishRelay properties
    // var isLoadingRelay: BehaviorRelay<Bool> { get set }
    // var errorMessage: BehaviorRelay<String?> { get set }
}

// MARK: - PresentableListener
protocol [ModuleName]PresentableListener: AnyObject {
    // TODO: Add PublishRelay properties for triggers
    // var triggerSomeAction: PublishRelay<SomeInputType> { get }
}

// MARK: - Router
protocol [ModuleName]Router: AnyObject {
    // TODO: Add navigation methods
}

final class [ModuleName]ViewModel: [ModuleName]ViewModelType, [ModuleName]PresentableListener {

    // MARK: - Properties

    weak var presenter: [ModuleName]Presentable?
    weak var router: [ModuleName]Router?
    weak var listener: [ModuleName]PresentableListener?

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
    }

    private func configurePresenter() {
        // TODO: Subscribe to UseCase responses and update presenter
    }
}
```

### Builder Structure

```swift
import Swinject

final class [ModuleName]Builder {

    // MARK: - Properties

    private let container: Container

    // MARK: - Initialization

    init(container: Container) {
        self.container = container
    }

    // MARK: - Build

    func build() -> [ModuleName]ViewController {
        let viewController = [ModuleName]ViewController()
        let viewModel = [ModuleName]ViewModel(
            // TODO: Resolve UseCase dependencies from container
            // someUseCase: container.resolve(SomeUseCaseType.self)!
        )

        viewController.viewModel = viewModel
        viewModel.presenter = viewController

        return viewController
    }
}
import RxSwift
import RxCocoa

// MARK: - ViewModelType
protocol [ModuleName]ViewModelType: CTViewModelType {
    var presenter: [ModuleName]Presentable? { get set }
    var router: [ModuleName]Router? { get set }
    var listener: [ModuleName]PresentableListener? { get set }
}

// MARK: - Presentable
protocol [ModuleName]Presentable: AnyObject {
    var listener: [ModuleName]PresentableListener? { get set }
    // TODO: Add BehaviorRelay and PublishRelay properties
}

// MARK: - PresentableListener
protocol [ModuleName]PresentableListener: AnyObject {
    // TODO: Add PublishRelay properties for triggers
}

// MARK: - Router
protocol [ModuleName]Router: AnyObject {
    // TODO: Add navigation methods
}

final class [ModuleName]ViewModel: [ModuleName]ViewModelType, [ModuleName]PresentableListener {

    // MARK: - Properties

    weak var presenter: [ModuleName]Presentable?
    weak var router: [ModuleName]Router?
    weak var listener: [ModuleName]PresentableListener?

    // TODO: Add UseCase dependencies
    let disposeBag = DisposeBag()

    // MARK: - Initialization

    init(
        // TODO: Add UseCase dependencies
    ) {
        // TODO: Initialize dependencies
    }

    // MARK: - Life Cycle

    func didBecomeActive() {
        presenter?.listener = self
        configureListener()
        configurePresenter()
    }

    // MARK: - Private Methods

    private func configureListener() {
        // TODO: Configure listener bindings
    }

    private func configurePresenter() {
        // TODO: Configure presenter bindings
    }
}
```

### Builder Structure

```swift
import UIKit

final class [ModuleName]Builder {

    // MARK: - Build

    static func build(listener: [ModuleName]PresentableListener? = nil) -> UIViewController {
        let viewModel = [ModuleName]ViewModel(
            // TODO: Add UseCase dependencies
        )
        let viewController = [ModuleName]ViewController()
        let router = [ModuleName]Router(viewController: viewController)

        // Setup dependencies
        viewModel.presenter = viewController
        viewModel.router = router
        viewModel.listener = listener
        viewController.viewModel = viewModel

        return viewController
    }
}
```

Keep all implementations minimal with clear TODO guidance.
