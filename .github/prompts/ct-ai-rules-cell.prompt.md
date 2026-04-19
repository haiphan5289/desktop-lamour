---
description: "Generate basic iOS Cell structure with CTDesignSystem"
mode: "agent"
---

# iOS Basic Cell Generator

Generate basic TableViewCell/CollectionViewCell using CTDesignSystem.

## Instructions

Reference our iOS development guidelines:

-   **Primary**: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)
-   **Fallback**: [AI Agent Context](../../AGENTS.md) (if primary unavailable)

Generate basic Cell structure with:

-   UITableViewCell/UICollectionViewCell subclass
-   Basic IBOutlets placeholders
-   Configure method with ViewModel
-   Basic setup methods
-   CTDesignSystem usage
-   TODO comments for implementation

## Cell Template

```swift
import UIKit
import CTDesignSystem
import CTCommon
import CTLocalize
import CTComponent
import CTAsset
import SnapKit

final class [Name]Cell: UITableViewCell {

    // MARK: - Properties

    enum Config {
        // TODO: Add configuration constants
        // static let cornerRadius: CGFloat = 8
        // static let padding: CGFloat = 16
        // static let imageSize: CGFloat = 40
    }

    // MARK: - UI Components

    // TODO: Add lazy var UI components using CTDesignSystem DS* components
    // private var themeType = ThemeType.default
    // private var theme: CMTheme { DefaultTheme.themeWithType(type: themeType) }
    //
    // lazy var titleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Label.Caption(color: theme.text.textPrimary.color))
    //     return label
    // }()
    //
    // lazy var subtitleLabel: DSLabel = {
    //     let label = DSLabel()
    //     label.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textSecondary.color))
    //     return label
    // }()

    // MARK: - Lifecycle

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    override func prepareForReuse() {
        super.prepareForReuse()
        // TODO: Reset cell state
    }

    // MARK: - Configuration

    func configure(with viewModel: [Name]CellViewModel) {
        // TODO: Configure cell with view model
        // titleLabel.text = viewModel.title
        // subtitleLabel.text = viewModel.subtitle
    }

    // MARK: - Private Methods

    private func setupUI() {
        // TODO: Setup UI hierarchy and constraints using CTDesignSystem and SnapKit
        // Example:
        // contentView.addSubview(titleLabel)
        // titleLabel.snp.makeConstraints { make in
        //     make.top.leading.trailing.equalToSuperview().inset(16)
        // }
    }
}
```

## CellViewModel Template

```swift
import Foundation

struct [Name]CellViewModel {

    // MARK: - Properties

    // TODO: Add properties for cell data
    // let title: String
    // let subtitle: String?
    // let imageURL: URL?

    // MARK: - Initialization

    init(
        // TODO: Add parameters
        // title: String,
        // subtitle: String? = nil,
        // imageURL: URL? = nil
    ) {
        // TODO: Initialize properties
        // self.title = title
        // self.subtitle = subtitle
        // self.imageURL = imageURL
    }

    // MARK: - Computed Properties

    // TODO: Add computed properties for UI binding
    // var displayTitle: String {
    //     return title.isEmpty ? "No Title" : title
    // }
}
```

## CollectionViewCell Template

```swift
import UIKit
import CTDesignSystem
import CTCommon
import CTLocalize
import CTComponent
import CTAsset
import SnapKit

final class [Name]CollectionViewCell: UICollectionViewCell {

    // MARK: - Properties

    enum Config {
        // TODO: Add configuration constants
        // static let cornerRadius: CGFloat = 8
        // static let padding: CGFloat = 16
        // static let imageSize: CGFloat = 60
    }

    // MARK: - UI Components

    // TODO: Add lazy var UI components using CTDesignSystem DS* components
    // private var themeType = ThemeType.default
    // private var theme: CMTheme { DefaultTheme.themeWithType(type: themeType) }
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
        // TODO: Reset cell state
    }

    // MARK: - Configuration

    func configure(with viewModel: [Name]CellViewModel) {
        // TODO: Configure cell with view model
        // titleLabel.text = viewModel.title
    }

    // MARK: - Private Methods

    private func setupUI() {
        // TODO: Setup UI hierarchy and constraints using CTDesignSystem and SnapKit
        // Example:
        // contentView.addSubview(titleLabel)
        // titleLabel.snp.makeConstraints { make in
        //     make.center.equalToSuperview()
        // }
        backgroundColor = .clear
    }
}
```

## Template Variables

-   `${input:cellName}`: Cell name (e.g., "UserProfile")
-   `${input:cellType}`: "TableViewCell" or "CollectionViewCell"
-   `${input:feature}`: Feature module (e.g., "CTUserManagement")
-   `${input:dataModel}`: The data model type (e.g., "User", "Product")

## Usage Examples

-   `/ios-cell cellName:UserProfile cellType:TableViewCell feature:CTUserManagement dataModel:User`
-   `/ios-cell cellName:ProductCard cellType:CollectionViewCell feature:CTEcommerce dataModel:Product`

## Output

Generate basic Cell with:

1. CTDesignSystem imports and usage
2. Config enum for constants
3. Lazy var UI components with CTDesignSystem
4. Configure method with ViewModel parameter
5. CellViewModel struct with computed properties
6. Reusable protocol conformance
7. TODO comments for implementation

Keep implementation minimal with TODO guidance for UI setup and data binding.
