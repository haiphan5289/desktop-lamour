---
name: ct-unittest
description: Generate iOS unit test structure using Quick and Nimble with mock classes. Creates QuickSpec test files with beforeEach setup, BDD-style describe/context/it blocks, mock repository, mock presentable, and mock use case. Use when writing tests for ViewModels, UseCases, or Repositories. Follows Given-When-Then pattern.
---

# iOS Unit Test Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate unit test structure with Quick, Nimble, and mock classes.

## Input Format

```
CLASS_NAME: <Class being tested, e.g. "UserProfileViewModel">
FEATURE: <Feature module, e.g. "CTUserManagement">
TEST_TYPE: <viewModel | useCase | repository>
```

## Test Spec Template

```swift
import Foundation
import UIKit
import CTDesignSystem
import CTCommon
import RxSwift
import Quick
import Nimble

@testable import [FeatureModule]

final class [ClassName]Spec: QuickSpec {
    override func spec() {
        var sut: [ClassUnderTest]!
        var mockPresenter: Mock[ClassName]Presentable!
        var mockRepository: Mock[Repository]!
        // var mockRouter: Mock[Router]!
        // var mockUseCase: Mock[UseCase]!

        beforeEach {
            mockRepository = Mock[Repository]()
            mockPresenter = Mock[ClassName]Presentable()

            sut = [ClassUnderTest](
                // repository: mockRepository
            )

            mockPresenter.stubbedIsLoadingRelay = BehaviorRelay<Bool>(value: false)
            mockPresenter.stubbedListener = sut
            sut.presenter = mockPresenter
            sut.didBecomeActive()
        }

        describe("[ClassUnderTest]") {
            context("when initialized") {
                it("should set presenter's listener to the SUT") {
                    expect(mockPresenter.stubbedListener).to(beIdenticalTo(sut))
                }

                it("should configure initial state") {
                    expect(sut).toNot(beNil())
                }
            }

            context("when didBecomeActive is called") {
                it("should configure presenter and listener") {
                    expect(mockPresenter.stubbedListener).to(beIdenticalTo(sut))
                }
            }

            context("when [specific action] occurs") {
                it("should [expected behavior]") {
                    // Given
                    // When
                    // Then
                }
            }
        }
    }
}
```

## Mock Repository Template

```swift
import Foundation
import RxSwift

@testable import [FeatureModule]

final class Mock[RepositoryName]: [RepositoryName]Type {

    var invokedMethodName = false
    var invokedMethodNameCount = 0
    var invokedMethodNameParameters: ([ParameterType], [ParameterType])?
    var stubbedMethodNameResult: Observable<[ReturnType]>!

    func methodName(
        parameter1: [ParameterType],
        parameter2: [ParameterType]
    ) -> Observable<[ReturnType]> {
        invokedMethodName = true
        invokedMethodNameCount += 1
        invokedMethodNameParameters = (parameter1, parameter2)
        return stubbedMethodNameResult
    }
}
```

## Mock Presentable Template

```swift
import Foundation
import RxSwift
import RxRelay

@testable import [FeatureModule]

final class Mock[PresentableName]: [PresentableName] {

    // MARK: - Listener
    var invokedListenerSetter = false
    var invokedListener: [PresentableListener]?
    var stubbedListener: [PresentableListener]!

    var listener: [PresentableListener]? {
        set {
            invokedListenerSetter = true
            invokedListener = newValue
        }
        get { stubbedListener }
    }

    // MARK: - BehaviorRelay Properties

    var stubbedIsLoadingRelay: BehaviorRelay<Bool>!
    var isLoadingRelay: BehaviorRelay<Bool> {
        get { stubbedIsLoadingRelay }
        set { stubbedIsLoadingRelay = newValue }
    }

    var stubbedDataSource: BehaviorRelay<[DataModel]>!
    var dataSource: BehaviorRelay<[DataModel]> {
        get { stubbedDataSource }
        set { stubbedDataSource = newValue }
    }

    var stubbedErrorMessage: BehaviorRelay<String?>!
    var errorMessage: BehaviorRelay<String?> {
        get { stubbedErrorMessage }
        set { stubbedErrorMessage = newValue }
    }

    // MARK: - Methods

    var invokedMethodName = false
    var invokedMethodNameCount = 0

    func methodName(parameter: [ParameterType]) {
        invokedMethodName = true
        invokedMethodNameCount += 1
    }
}
```

## Mock UseCase Template

```swift
import Foundation
import RxSwift
import Action

@testable import [FeatureModule]

final class Mock[UseCaseName]: [UseCaseName]Type {

    var invokedActionGetter = false
    var stubbedAction: Action<[InputType], [OutputType]>?

    var action: Action<[InputType], [OutputType]>? {
        get {
            invokedActionGetter = true
            return stubbedAction
        }
        set { stubbedAction = newValue }
    }
}
```

## Test Organization Patterns

### BDD Structure
```swift
describe("UserProfileViewModel") {
    context("when user data is loaded") {
        it("should update the data source") { }
        it("should stop loading state") { }
    }

    context("when error occurs") {
        it("should display error message") { }
    }
}
```

### Given-When-Then
```swift
it("should handle successful login") {
    // Given
    let expectedUser = UserModel.mock()
    mockUseCase.stubbedRunResult = Observable.just(expectedUser)

    // When
    sut.login(email: "test@example.com", password: "password")

    // Then
    expect(mockPresenter.stubbedDataSource.value).to(equal(expectedUser))
    expect(mockPresenter.stubbedIsLoadingRelay.value).to(beFalse())
}
```

### Mock Verification
```swift
it("should call repository with correct parameters") {
    // Given
    let userID = "123"

    // When
    sut.loadUser(id: userID)

    // Then
    expect(mockRepository.invokedGetUser).to(beTrue())
    expect(mockRepository.invokedGetUserParameters?.userID).to(equal(userID))
}
```

## Rules

- All test files use `QuickSpec` with `override func spec()`
- Always initialize mocks in `beforeEach`
- Mock properties track invocation count (`invokedXCount`) and last params (`invokedXParameters`)
- Use `@testable import [FeatureModule]` for access
- Never test implementation details — test behavior only
- Use `beTrue()`, `beFalse()`, `beNil()`, `equal()`, `beIdenticalTo()` from Nimble
- Tests should not depend on order of execution
