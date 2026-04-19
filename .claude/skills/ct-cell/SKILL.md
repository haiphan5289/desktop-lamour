---
name: ct-cell
description: Generate a basic iOS TableViewCell or CollectionViewCell using CTDesignSystem. Creates the cell class with Config enum, CTDesignSystem UI components (DSLabel, DSButton), configure(with:) method, CellViewModel struct, and SnapKit constraints. Use when creating a new reusable cell.
---

# iOS Basic Cell Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate TableViewCell or CollectionViewCell using CTDesignSystem.

## Input Format

```
CELL_NAME: <Name, e.g. "UserProfile">
CELL_TYPE: <TableViewCell | CollectionViewCell>
FEATURE: <Module, e.g. "CTUserManagement">
DATA_MODEL: <Data model type, e.g. "User">
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
        // static let imageSize: CGFloat = 40
    }

    // MARK: - UI Components

    private var theme = CMStaticThemeLoader.defaultTheme

    lazy var titleLabel: DSLabel = {
        let label = DSLabel()
        label.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
        return label
    }()

    lazy var subtitleLabel: DSLabel = {
        let label = DSLabel()
        label.setStyle(DS.TypoToken.Body.Caption(color: theme.text.textSecondary.color))
        return label
    }()

    // MARK: - Lifecycle

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    // For programmatic cells (no XIB):
    // override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
    //     super.init(style: style, reuseIdentifier: reuseIdentifier)
    //     setupUI()
    // }
    //
    // required init?(coder: NSCoder) {
    //     super.init(coder: coder)
    // }

    override func prepareForReuse() {
        super.prepareForReuse()
        titleLabel.text = nil
        subtitleLabel.text = nil
    }

    // MARK: - Configuration

    func configure(with viewModel: [Name]CellViewModel) {
        titleLabel.text = viewModel.title
        subtitleLabel.text = viewModel.subtitle
    }

    // MARK: - Private Methods

    private func setupUI() {
        selectionStyle = .none
        contentView.addSubview(titleLabel)
        contentView.addSubview(subtitleLabel)

        titleLabel.snp.makeConstraints { make in
            make.top.leading.trailing.equalToSuperview().inset(16)
        }

        subtitleLabel.snp.makeConstraints { make in
            make.top.equalTo(titleLabel.snp.bottom).offset(4)
            make.leading.trailing.bottom.equalToSuperview().inset(16)
        }
    }
}

// MARK: - CellViewModel
struct [Name]CellViewModel {

    let title: String
    let subtitle: String?

    init(title: String, subtitle: String? = nil) {
        self.title = title
        self.subtitle = subtitle
    }
}
```

## CollectionViewCell Template

```swift
import UIKit
import CTDesignSystem
import CTCommon
import SnapKit

final class [Name]CollectionViewCell: UICollectionViewCell {

    // MARK: - Properties

    enum Config {
        // static let cornerRadius: CGFloat = 8
        // static let padding: CGFloat = 16
    }

    static let reuseIdentifier = "[Name]CollectionViewCell"

    // MARK: - UI Components

    private var theme = CMStaticThemeLoader.defaultTheme

    lazy var titleLabel: DSLabel = {
        let label = DSLabel()
        label.setStyle(DS.TypoToken.Label.Section(color: theme.text.textPrimary.color))
        return label
    }()

    // MARK: - Lifecycle

    override func awakeFromNib() {
        super.awakeFromNib()
        setupUI()
    }

    override func prepareForReuse() {
        super.prepareForReuse()
        titleLabel.text = nil
    }

    // MARK: - Configuration

    func configure(with viewModel: [Name]CellViewModel) {
        titleLabel.text = viewModel.title
    }

    // MARK: - Private Methods

    private func setupUI() {
        contentView.addSubview(titleLabel)

        titleLabel.snp.makeConstraints { make in
            make.center.equalToSuperview()
            make.edges.equalToSuperview().inset(8)
        }

        backgroundColor = .clear
    }
}

// MARK: - CellViewModel
struct [Name]CellViewModel {
    let title: String
}
```

## Theme Selection by Module

```swift
// Default (generic)
private var theme = CMStaticThemeLoader.defaultTheme

// Job module
private var theme = CMStaticThemeLoader.jobTheme

// Property module
private var theme = CMStaticThemeLoader.ptyTheme
```

## Rules

- **ALWAYS** use `DSLabel`, `DSButton`, `DSImageView` — never `UILabel`, `UIButton`
- **ALWAYS** use SnapKit for constraints
- Use `Config` enum for layout constants
- Use `selectionStyle = .none` for TableViewCells (unless selection is needed)
- Reset all configurable content in `prepareForReuse`
- CollectionViewCells need a static `reuseIdentifier`
- `CellViewModel` is a struct in the same file
- Match theme loader to module type (`defaultTheme`, `jobTheme`, `ptyTheme`)
