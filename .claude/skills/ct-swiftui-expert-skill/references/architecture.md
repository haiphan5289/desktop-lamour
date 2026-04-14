# MVVM + Combine Architecture in ChoTot

ChoTot uses a standardized MVVM pattern powered by Combine and a type-erased `AnyViewModel`.

## 1. State Struct
Defines the data the view needs to render. It should only contain simple types or models.

```swift
struct MyFeatureState {
    var items: [Item] = []
    var isLoading: Bool = false
    var errorMessage: String?
}
```

## 2. Input Enum
Defines all possible actions/events the user can trigger from the view.

```swift
enum MyFeatureInput {
    case fetchData
    case didSelectItem(Item)
    case retry
}
```

## 3. ViewModel Implementation
The ViewModel handles business logic and updates the state.

```swift
class MyFeatureViewModel: ViewModel {
    @Injected(\.myUseCase) var myUseCase
    
    @Published var state = MyFeatureState()
    
    private var cancellables = Set<AnyCancellable>()
    private let fetchDataStream = PassthroughRelay<Void>()
    
    init() {
        setupBindings()
    }
    
    private func setupBindings() {
        fetchDataStream
            .handleEvents(receiveOutput: { [weak self] _ in self?.state.isLoading = true })
            .flatMap { [weak self] _ in
                self?.myUseCase.run() ?? Empty().eraseToAnyPublisher()
            }
            .sink { [weak self] result in
                self?.state.isLoading = false
                self?.state.items = result
            }
            .store(in: &cancellables)
    }
    
    func trigger(_ input: MyFeatureInput) {
        switch input {
        case .fetchData: fetchDataStream.accept()
        // ... handle other inputs
        }
    }
}
```

## 4. View Integration
The View interacts with the ViewModel through `AnyViewModel`.

```swift
struct MyFeatureScreen: View {
    @ObservedObject var viewModel: AnyViewModel<MyFeatureState, MyFeatureInput>
    
    var body: some View {
        VStack {
            if viewModel.isLoading {
                ProgressView()
            } else {
                List(viewModel.items) { item in
                    Text(item.name)
                }
            }
        }
        .onAppear { viewModel.trigger(.fetchData) }
    }
}
```
