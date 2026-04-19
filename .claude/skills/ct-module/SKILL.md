---
name: ct-module
description: Generate a complete MVVM-C module structure with ViewController, ViewModel, and Builder files. Use when creating a new feature module from scratch. Generates all three files with protocol definitions, RxSwift patterns, CTDesignSystem usage, Swinject DI setup, and TODO guidance.
---

# iOS Basic Module Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate complete MVVM-C module with barebone structure following production patterns.

## Input Format

```
MODULE_NAME: <ModuleName, e.g. "UserProfile">
FEATURE: <Feature module, e.g. "CTUserManagement">
```

## Output Files

1. `[ModuleName]ViewController.swift` — UI layer with CTDesignSystem
2. `[ModuleName]ViewModel.swift` — Business logic with UseCase dependencies + all protocols
3. `[ModuleName]Builder.swift` — Dependency injection setup

## ViewController.swift

```swift
import UIKit
import CTDesignSystem
import CTCommon
import RxSwift
import RxRelay
import SnapKit

final class [ModuleName]ViewController: UIViewController, [ModuleName]Presentable {

    // MARK: - Properties

    enum Config {
        // static let standardSize: CGFloat = 44
        // static let padding: CGFloat = 16
    }

    var viewModel: [ModuleName]ViewModelType?
    weak var listener: [ModuleName]PresentableListener?

    // var isLoadingRelay = BehaviorRelay<Bool>(value: false)
    // var errorMessage = BehaviorRelay<String?>(value: nil)

    let disposeBag = DisposeBag()

    // MARK: - UI Components

    // private var theme = CMStaticThemeLoader.defaultTheme
    //
    // lazy var titleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))
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
    }

    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
    }

    deinit {
        Logger.print("\(self) deallocated.")
        NotificationCenter.default.removeObserver(self)
    }

    // MARK: - Private Methods

    private func setupViews() {
        // view.addSubview(titleLabel)
        // titleLabel.snp.makeConstraints { make in
        //     make.edges.equalToSuperview().inset(16)
        // }
    }

    private func setupActions() { }

    private func configurePresenter() {
        // Bind presenter relays to UI
    }

    private func configureViewModel() {
        viewModel?.didBecomeActive()
    }
}
```

## ViewModel.swift (includes all protocols)

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
    // var isLoadingRelay: BehaviorRelay<Bool> { get set }
    // var datasource: BehaviorRelay<[SomeModel]> { get set }
}

// MARK: - PresentableListener
protocol [ModuleName]PresentableListener: AnyObject {
    // var triggerSomeAction: PublishRelay<SomeInputType> { get }
}

// MARK: - Router
protocol [ModuleName]Router: AnyObject {
    // func navigateToSomeScreen()
}

final class [ModuleName]ViewModel: [ModuleName]ViewModelType, [ModuleName]PresentableListener {

    // MARK: - Properties

    weak var presenter: [ModuleName]Presentable?
    weak var router: [ModuleName]Router?
    weak var listener: [ModuleName]PresentableListener?

    // private let someUseCase: SomeUseCaseType
    let disposeBag = DisposeBag()

    // MARK: - Initialization

    init(
        // someUseCase: SomeUseCaseType
    ) {
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
        // presenter?.triggerSomeAction
        //     .subscribeNext { [weak self] input in
        //         self?.handleSomeAction(input)
        //     }.disposed(by: disposeBag)
    }

    private func configurePresenter() {
        // someUseCase.action?.elements
        //     .observe(on: MainScheduler.instance)
        //     .subscribeNext { [weak self] result in
        //         self?.presenter?.datasource.accept(result)
        //     }.disposed(by: disposeBag)
    }
}
```

## Builder.swift

```swift
import UIKit
import Swinject

final class [ModuleName]Builder {

    // MARK: - Build

    static func build(listener: [ModuleName]PresentableListener? = nil) -> UIViewController {
        let viewModel = [ModuleName]ViewModel(
            // Resolve UseCase dependencies from container or create inline
        )
        let viewController = [ModuleName]ViewController()
        let router = [ModuleName]RouterImpl(viewController: viewController)

        viewModel.presenter = viewController
        viewModel.router = router
        viewModel.listener = listener
        viewController.viewModel = viewModel

        return viewController
    }
}
```

## Rules

- All 3 files must be created together
- `ViewController` conforms to `Presentable` protocol
- `ViewModel` conforms to `ViewModelType` and `PresentableListener`
- Protocols are defined in the `ViewModel` file
- Use `Logger.print("\(self) deallocated.")` in ViewController `deinit`
- `configureViewModel()` calls `viewModel?.didBecomeActive()`
- Use SnapKit for all constraints, never NSLayoutConstraint
- Only use DSLabel, DSButton — never UILabel, UIButton directly
