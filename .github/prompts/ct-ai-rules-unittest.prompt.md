---
description: "Generate iOS unit test structure using Quick and Nimble"
mode: "agent"
---

# iOS Unit Test Generator

Generate unit test structure following Quick and Nimble patterns with mock generation.

## Instructions

Reference our iOS development guidelines: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)

Generate unit test structure with:

-   Quick and Nimble testing framework
-   Mock classes for all dependencies
-   Spec test structure with proper setup
-   BDD-style test organization
-   Proper imports and test configuration

## Test Spec Template

```swift
import Foundation
import UIKit
import CTDesignSystem
import CTCommon
import CTLocalize
import CTComponent
import CTAsset
import RxSwift
import Quick
import Nimble
import CTTracking

@testable import [FeatureModule]

final class [TestClassName]Spec: QuickSpec {
    override func spec() {
        var sut: [ClassUnderTest]!
        var mockPresenter: Mock[ClassUnderTest]Presentable!
        var mockRepository: Mock[Repository]!
        // TODO: Add other mock dependencies
        // var mockRouter: Mock[Router]!
        // var mockUseCase: Mock[UseCase]!

        beforeEach {
            // TODO: Initialize mock dependencies
            mockRepository = Mock[Repository]()
            mockPresenter = Mock[ClassUnderTest]Presentable()

            // TODO: Initialize system under test with dependencies
            sut = [ClassUnderTest](
                // TODO: Add constructor parameters
                // repository: mockRepository,
                // useCase: mockUseCase
            )

            // TODO: Setup mock presenter properties
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
                    // TODO: Add initialization tests
                    expect(sut).toNot(beNil())
                }
            }

            context("when didBecomeActive is called") {
                it("should configure presenter and listener") {
                    // TODO: Add didBecomeActive tests
                    expect(mockPresenter.stubbedListener).to(beIdenticalTo(sut))
                }
            }

            // TODO: Add more test contexts for business logic
            context("when [specific action] occurs") {
                it("should [expected behavior]") {
                    // TODO: Add specific test cases
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

    // TODO: Add mock properties for each repository method

    var invokedMethodName = false
    var invokedMethodNameCount = 0
    var invokedMethodNameParameters: ([ParameterType], [ParameterType])?
    var invokedMethodNameParametersList = [([ParameterType], [ParameterType])]()
    var stubbedMethodNameResult: Observable<[ReturnType]>!

    func methodName(
        parameter1: [ParameterType],
        parameter2: [ParameterType]
    ) -> Observable<[ReturnType]> {
        invokedMethodName = true
        invokedMethodNameCount += 1
        invokedMethodNameParameters = (parameter1, parameter2)
        invokedMethodNameParametersList.append((parameter1, parameter2))
        return stubbedMethodNameResult
    }

    // TODO: Add more repository methods following the same pattern
}
```

## Mock Presentable Template

```swift
import Foundation
import RxSwift
import RxRelay

@testable import [FeatureModule]

final class Mock[PresentableName]: [PresentableName] {

    // MARK: - Listener Property
    var invokedListenerSetter = false
    var invokedListenerSetterCount = 0
    var invokedListener: [PresentableListener]?
    var invokedListenerList = [[PresentableListener]?]()
    var invokedListenerGetter = false
    var invokedListenerGetterCount = 0
    var stubbedListener: [PresentableListener]!

    var listener: [PresentableListener]? {
        set {
            invokedListenerSetter = true
            invokedListenerSetterCount += 1
            invokedListener = newValue
            invokedListenerList.append(newValue)
        }
        get {
            invokedListenerGetter = true
            invokedListenerGetterCount += 1
            return stubbedListener
        }
    }

    // MARK: - BehaviorRelay Properties

    // TODO: Add BehaviorRelay properties for data binding
    var invokedDataSourceSetter = false
    var invokedDataSourceSetterCount = 0
    var invokedDataSource: BehaviorRelay<[DataModel]>?
    var invokedDataSourceList = [BehaviorRelay<[DataModel]>]()
    var invokedDataSourceGetter = false
    var invokedDataSourceGetterCount = 0
    var stubbedDataSource: BehaviorRelay<[DataModel]>!

    var dataSource: BehaviorRelay<[DataModel]> {
        set {
            invokedDataSourceSetter = true
            invokedDataSourceSetterCount += 1
            invokedDataSource = newValue
            invokedDataSourceList.append(newValue)
        }
        get {
            invokedDataSourceGetter = true
            invokedDataSourceGetterCount += 1
            return stubbedDataSource
        }
    }

    var invokedIsLoadingRelaySetter = false
    var invokedIsLoadingRelaySetterCount = 0
    var invokedIsLoadingRelay: BehaviorRelay<Bool>?
    var invokedIsLoadingRelayList = [BehaviorRelay<Bool>]()
    var invokedIsLoadingRelayGetter = false
    var invokedIsLoadingRelayGetterCount = 0
    var stubbedIsLoadingRelay: BehaviorRelay<Bool>!

    var isLoadingRelay: BehaviorRelay<Bool> {
        set {
            invokedIsLoadingRelaySetter = true
            invokedIsLoadingRelaySetterCount += 1
            invokedIsLoadingRelay = newValue
            invokedIsLoadingRelayList.append(newValue)
        }
        get {
            invokedIsLoadingRelayGetter = true
            invokedIsLoadingRelayGetterCount += 1
            return stubbedIsLoadingRelay
        }
    }

    var invokedErrorMessageSetter = false
    var invokedErrorMessageSetterCount = 0
    var invokedErrorMessage: BehaviorRelay<String?>?
    var invokedErrorMessageList = [BehaviorRelay<String?>]()
    var invokedErrorMessageGetter = false
    var invokedErrorMessageGetterCount = 0
    var stubbedErrorMessage: BehaviorRelay<String?>!

    var errorMessage: BehaviorRelay<String?> {
        set {
            invokedErrorMessageSetter = true
            invokedErrorMessageSetterCount += 1
            invokedErrorMessage = newValue
            invokedErrorMessageList.append(newValue)
        }
        get {
            invokedErrorMessageGetter = true
            invokedErrorMessageGetterCount += 1
            return stubbedErrorMessage
        }
    }

    // MARK: - PublishRelay Properties

    // TODO: Add PublishRelay properties for triggers
    var invokedTriggerActionSetter = false
    var invokedTriggerActionSetterCount = 0
    var invokedTriggerAction: PublishRelay<[TriggerType]>?
    var invokedTriggerActionList = [PublishRelay<[TriggerType]>]()
    var invokedTriggerActionGetter = false
    var invokedTriggerActionGetterCount = 0
    var stubbedTriggerAction: PublishRelay<[TriggerType]>!

    var triggerAction: PublishRelay<[TriggerType]> {
        set {
            invokedTriggerActionSetter = true
            invokedTriggerActionSetterCount += 1
            invokedTriggerAction = newValue
            invokedTriggerActionList.append(newValue)
        }
        get {
            invokedTriggerActionGetter = true
            invokedTriggerActionGetterCount += 1
            return stubbedTriggerAction
        }
    }

    // MARK: - Methods

    // TODO: Add method mocks for presentable actions
    var invokedMethodName = false
    var invokedMethodNameCount = 0
    var invokedMethodNameParameters: ([ParameterType], Void)?
    var invokedMethodNameParametersList = [([ParameterType], Void)]()

    func methodName(parameter: [ParameterType]) {
        invokedMethodName = true
        invokedMethodNameCount += 1
        invokedMethodNameParameters = (parameter, ())
        invokedMethodNameParametersList.append((parameter, ()))
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

    // MARK: - Action UseCase Mock
    var invokedActionSetter = false
    var invokedActionSetterCount = 0
    var invokedAction: Action<[InputType], [OutputType]>?
    var invokedActionList = [Action<[InputType], [OutputType]>?]()
    var invokedActionGetter = false
    var invokedActionGetterCount = 0
    var stubbedAction: Action<[InputType], [OutputType]>?

    var action: Action<[InputType], [OutputType]>? {
        set {
            invokedActionSetter = true
            invokedActionSetterCount += 1
            invokedAction = newValue
            invokedActionList.append(newValue)
        }
        get {
            invokedActionGetter = true
            invokedActionGetterCount += 1
            return stubbedAction
        }
    }

    // MARK: - Standard UseCase Mock
    var invokedRun = false
    var invokedRunCount = 0
    var invokedRunParameters: ([InputType], Void)?
    var invokedRunParametersList = [([InputType], Void)]()
    var stubbedRunResult: Observable<[OutputType]>!

    func run(input: [InputType]) -> Observable<[OutputType]> {
        invokedRun = true
        invokedRunCount += 1
        invokedRunParameters = (input, ())
        invokedRunParametersList.append((input, ()))
        return stubbedRunResult
    }
}
```

## Template Variables

-   `${input:className}`: Class name being tested (e.g., "UserProfileViewModel")
-   `${input:feature}`: Feature module (e.g., "CTUserManagement")
-   `${input:testType}`: Test type: "viewModel", "useCase", "repository"

## Usage Examples

-   `/ios-unittest className:UserProfileViewModel feature:CTUserManagement testType:viewModel`
-   `/ios-unittest className:GetUserUseCase feature:CTUserManagement testType:useCase`
-   `/ios-unittest className:UserRepository feature:CTUserManagement testType:repository`

## Test Organization Best Practices

### 1. BDD Structure

```swift
describe("UserProfileViewModel") {
    context("when user data is loaded") {
        it("should update the data source") {
            // Test implementation
        }

        it("should stop loading state") {
            // Test implementation
        }
    }

    context("when error occurs") {
        it("should display error message") {
            // Test implementation
        }
    }
}
```

### 2. Given-When-Then Pattern

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

### 3. Mock Verification

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

## Output

Generate unit test with:

1. Quick and Nimble test structure
2. Mock classes for all dependencies
3. Proper beforeEach setup
4. BDD-style test organization
5. TODO comments for test implementation
6. Proper import statements
7. Mock verification patterns

Keep tests focused on behavior verification without business logic implementation.
