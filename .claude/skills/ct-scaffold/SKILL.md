---
name: ct-scaffold
description: Scaffold basic barebone iOS files following MVVM-C architecture. Use when creating a ViewController, ViewModel, UseCase, Repository, Service, Model, or Cell file from scratch. Generates proper MARK sections, imports, protocol structure, RxSwift patterns, CTDesignSystem usage, and TODO comments. Supports: ViewController, ViewModel, UseCase, Repository, Service, Model, TableViewCell, CollectionViewCell.
---

# iOS Basic File Scaffolding

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Create basic barebone iOS files following MVVM-C architecture and coding conventions.

## Input Format

```
FILE_TYPE: <ViewController | ViewModel | UseCase | Repository | Service | Model | TableViewCell | CollectionViewCell>
NAME: <BaseName, e.g. "UserProfile">
MODULE: <Module name, e.g. "CTUserManagement">
```

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
import RxSwift
import RxRelay
import SnapKit

final class [Name]ViewController: UIViewController, [Name]Presentable {

    // MARK: - Properties

    enum Config {
        // static let standardSize: CGFloat = 44
        // static let padding: CGFloat = 16
    }

    var viewModel: [Name]ViewModelType?
    weak var listener: [Name]PresentableListener?

    // var isLoadingRelay = BehaviorRelay<Bool>(value: false)
    // var errorMessage = BehaviorRelay<String?>(value: nil)
    // var triggerSomeAction = PublishRelay<Void>()

    let disposeBag = DisposeBag()

    // MARK: - UI Components

    // private var themeType = ThemeType.default
    // private var theme: CMTheme { DefaultTheme.themeWithType(type: themeType) }
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
        NotificationCenter.default.removeObserver(self)
    }

    // MARK: - Private Methods

    private func setupViews() {
        // view.addSubview(someView)
        // someView.snp.makeConstraints { make in
        //     make.edges.equalToSuperview()
        // }
    }

    private func setupActions() { }

    private func configurePresenter() { }

    private func configureViewModel() { }
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
    // var isLoadingRelay: BehaviorRelay<Bool> { get set }
    // var datasource: BehaviorRelay<[SomeModel]> { get set }
}

// MARK: - PresentableListener
protocol [Name]PresentableListener: AnyObject {
    // var triggerSomeAction: PublishRelay<SomeInputType> { get }
}

// MARK: - Router
protocol [Name]Router: AnyObject {
    // func navigateToSomeScreen()
}

final class [Name]ViewModel: [Name]ViewModelType, [Name]PresentableListener {

    // MARK: - Properties

    weak var presenter: [Name]Presentable?
    weak var router: [Name]Router?
    weak var listener: [Name]PresentableListener?

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
        // presenter?.triggerSomeAction.subscribeNext { [weak self] input in
        //     self?.handleSomeAction(input)
        // }.disposed(by: disposeBag)
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

## UseCase Template

```swift
import RxSwift
import Action
import CTCommon

protocol [Name]UseCaseType {
    var action: Action<[InputType], [OutputType]>? { get set }
}

final class [Name]UseCase: [Name]UseCaseType {

    var action: Action<[InputType], [OutputType]>?

    private let repository: [Name]RepositoryType

    init(repository: [Name]RepositoryType) {
        self.repository = repository
        action = Action { [weak self] input in
            guard let self = self else { return .empty() }
            return self.repository.someMethod(input: input)
        }
    }
}
```

## Repository Template

```swift
import Foundation
import RxSwift
import CTCommon

protocol [Name]RepositoryType: AnyObject {
    // func getSomeData(parameter: String) -> Observable<SomeModel>
}

class [Name]Repository: NSObject, [Name]RepositoryType {

    // MARK: - Properties

    let service: [Name]ServiceType

    // MARK: - Initialization

    init(service: [Name]ServiceType) {
        self.service = service
    }

    // MARK: - [Name]RepositoryType

    // func getSomeData(parameter: String) -> Observable<SomeModel> {
    //     service.getSomeData(parameter: parameter)
    //         .compactMap { $0 }
    // }
}
```

## Service Template

```swift
import Foundation
import RxSwift
import CTApiClient

protocol [Name]ServiceType {
    // func fetchSomeData(parameter: String) -> Observable<SomeModel?>
}

struct [Name]Service: [Name]ServiceType {

    // MARK: - [Name]ServiceType

    // func fetchSomeData(parameter: String) -> Observable<SomeModel?> {
    //     [Name]Targets.FetchData(parameter: parameter)
    //         .execute()
    //         .observe(on: MainScheduler.instance)
    // }
}
```

## TableViewCell Template

```swift
import UIKit
import CTDesignSystem
import CTCommon
import SnapKit

final class [Name]Cell: UITableViewCell {

    // MARK: - Properties

    enum Config {
        // static let cornerRadius: CGFloat = 8
        // static let padding: CGFloat = 16
    }

    // MARK: - UI Components

    // private var theme = CMStaticThemeLoader.defaultTheme
    //
    // lazy var titleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))
    //     return label
    // }()

    // MARK: - Lifecycle

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    override func prepareForReuse() {
        super.prepareForReuse()
    }

    // MARK: - Configuration

    func configure(with viewModel: [Name]CellViewModel) {
        // titleLabel.text = viewModel.title
    }

    // MARK: - Private Methods

    private func setupUI() {
        // contentView.addSubview(titleLabel)
        // titleLabel.snp.makeConstraints { make in
        //     make.edges.equalToSuperview().inset(16)
        // }
    }
}

struct [Name]CellViewModel {
    // let title: String
    // let subtitle: String?
}
```

## Rules

- **ALWAYS** use CTDesignSystem (`DSLabel`, `DSButton`, etc.) — never raw UIKit
- **ALWAYS** use SnapKit for layout constraints — never NSLayoutConstraint
- Use `BehaviorRelay` for state, `PublishRelay` for events
- Use `[weak self]` in all closures
- Include `deinit` with `Logger.print("\(self) deallocated.")` in ViewControllers
- Add `disposed(by: disposeBag)` for all RxSwift subscriptions
