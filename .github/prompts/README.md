@@ -0,0 +1,230 @@
# iOS Scaffolding Guide

This guide explains how to use the iOS prompt files to generate code scaffolding for your MVVM+C architecture project.

## Available Prompts

### 1. **ios-scaffold.prompt.md** - Basic File Scaffolding
Generate individual files like ViewControllers, ViewModels, etc.

### 2. **ios-module.prompt.md** - Complete Module Generation
Generate complete MVVM+C modules with ViewController, ViewModel, and Builder.

### 3. **ios-usecase.prompt.md** - UseCase Generation
Generate Clean Architecture use cases with repository dependencies.

### 4. **ios-repository.prompt.md** - Repository Generation
Generate repositories with service layer integration.

### 5. **ios-target.prompt.md** - API Target Generation
Generate API targets following Requestable protocol patterns.

### 6. **ios-cell.prompt.md** - Cell Generation
Generate TableView/CollectionView cells with AppDesignSystem.

### 7. **ios-unittest.prompt.md** - Unit Test Generation
Generate unit tests using Quick and Nimble with mock classes.

## How to Use Prompts

### Method 1: GitHub Copilot Chat Commands

Use `/` commands in GitHub Copilot Chat:

```
/ios-scaffold fileName:UserProfile fileType:ViewController
/ios-module moduleName:UserProfile featureName:CTUserManagement
/ios-usecase useCaseName:GetUserProfile feature:CTUserManagement useCaseType:action
/ios-repository repositoryName:UserProfile feature:CTUserManagement
/ios-target targetName:UserProfile feature:CTUserManagement operations:get,create
/ios-cell cellName:UserProfile feature:CTUserManagement
/ios-unittest className:UserProfileViewModel feature:CTUserManagement testType:viewModel
```

### Method 2: Natural Language Requests

Ask Copilot to generate code using natural language:

```
"Generate a UserProfile ViewController using our iOS scaffold template"
"Create a complete MVVM+C module for UserProfile in CTUserManagement feature"
"Generate a UseCase for getting user profile data"
"Create a repository for UserProfile with service integration"
"Generate API targets for UserProfile CRUD operations"
"Create a UserProfile table view cell with AppDesignSystem"
"Generate unit tests for UserProfileViewModel with Quick and Nimble"
```

## Template Variables

All prompts support these variables:

- `${input:fileName}` / `${input:moduleName}` - Base name (e.g., "UserProfile")
- `${input:featureName}` - Feature module (e.g., "CTUserManagement")
- `${input:fileType}` - File type (ViewController, ViewModel, UseCase, etc.)
- `${input:useCaseType}` - "action" or "standard" for UseCase
- `${input:operations}` - Comma-separated operations for API targets

## Usage Examples

### Complete Feature Development Flow

1. **Start with API Target**:
```
/ios-target targetName:UserProfile feature:CTUserManagement operations:get,update,delete
```

2. **Create Repository**:
```
/ios-repository repositoryName:UserProfile feature:CTUserManagement
```

3. **Generate Use Cases**:
```
/ios-usecase useCaseName:GetUserProfile feature:CTUserManagement useCaseType:action
/ios-usecase useCaseName:UpdateUserProfile feature:CTUserManagement useCaseType:action
```

4. **Create Complete Module**:
```
/ios-module moduleName:UserProfile featureName:CTUserManagement
```

5. **Add Custom Cell (if needed)**:
```
/ios-cell cellName:UserProfileItem feature:CTUserManagement
```

### Individual File Generation

**ViewController Only**:
```
/ios-scaffold fileName:UserProfile fileType:ViewController
```

**ViewModel Only**:
```
/ios-scaffold fileName:UserProfile fileType:ViewModel
```

**UseCase Only**:
```
/ios-usecase useCaseName:ValidateUserInput feature:CTUserManagement useCaseType:standard
```

**Unit Test Only**:
```
/ios-unittest className:UserProfileViewModel feature:CTUserManagement testType:viewModel
```

## Project Structure

Generated files should be organized in your project like this:

```
Features/
  CTUserManagement/
    UserProfile/
      UserProfileViewController.cs
      UserProfileViewModel.cs
      UserProfileBuilder.cs
    UseCase/
      GetUserProfileUseCase.cs
      UpdateUserProfileUseCase.cs
    Repository/
      UserProfileRepository.cs
    Target/
      UserProfileTarget.cs
    Cell/
      UserProfileItemCell.cs
      UserProfileItemCellViewModel.cs
ChoTotTests/
  CTUserManagement/
    UserProfileViewModelSpec.cs
    GetUserProfileUseCaseSpec.cs
    UserProfileRepositorySpec.cs
```

## Best Practices

### 1. **Follow Naming Conventions**
- Use PascalCase for class names: `UserProfileViewController`
- Use descriptive names: `GetUserProfileUseCase` instead of `UserUseCase`
- Include feature prefix when needed: `CTUserManagementConfig`

### 2. **Generate in Order**
1. API Targets (lowest level)
2. Repositories
3. Use Cases
4. ViewModels & ViewControllers
5. Supporting files (Cells, etc.)
6. Unit Tests (after implementation)

### 3. **Customize After Generation**
- All generated files contain TODO comments
- Replace placeholder types with actual models
- Implement business logic in marked sections
- Add proper imports based on your needs

### 4. **Use AppDesignSystem**
- Always use AppDesignSystem for UI components
- Follow the examples in generated templates
- Don't use WPF components directly

## Common Commands Reference

### Quick Module Setup
```bash
# Generate complete module with all dependencies
/ios-module moduleName:ProductListing featureName:CTEcommerce

# Add supporting use cases
/ios-usecase useCaseName:SearchProducts feature:CTEcommerce useCaseType:action
/ios-usecase useCaseName:FilterProducts feature:CTEcommerce useCaseType:standard

# Add custom cell
/ios-cell cellName:ProductItem feature:CTEcommerce
```

### API Integration Setup
```bash
# Generate API layer
/ios-target targetName:Product feature:CTEcommerce operations:get,search,filter
/ios-repository repositoryName:Product feature:CTEcommerce
/ios-usecase useCaseName:GetProduct feature:CTEcommerce useCaseType:action
```

### UI Component Setup
```bash
# Generate UI components
/ios-scaffold fileName:ProductDetail fileType:ViewController
/ios-cell cellName:ProductImage feature:CTEcommerce
/ios-cell cellName:ProductInfo feature:CTEcommerce
```

## Troubleshooting

### Common Issues

1. **Prompt not recognized**: Ensure you're using GitHub Copilot Chat and the prompt files are in `.github/prompts/`

2. **Missing imports**: Add required imports based on your feature dependencies

3. **Build errors**: Replace placeholder types with actual models from your project

4. **Design system not found**: Ensure AppDesignSystem is properly imported in your project

### Getting Help

- Reference the iOS general instructions: `.github/instructions/ios-general-instructions.instructions.md`
- Check existing code in your feature modules for patterns
- Follow the TODO comments in generated code
- Use AppDesignSystem documentation for UI components

## Tips

- Start with a complete module using `/ios-module` then add specific use cases
- Use descriptive names that include the feature context
- Always implement TODO comments before moving to the next component
- Test your generated code incrementally
- Follow the MVVM+C architecture patterns shown in the templates
