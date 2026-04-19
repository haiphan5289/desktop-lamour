# Use Case Generation Prompt with Serena Integration

Goal: Generate a complete use case implementation following MVVM + Clean Architecture using Serena's semantic understanding.

## Enhanced Workflow with Serena

### 1. Analysis Phase

```bash
# Analyze similar existing use cases
make serena-analyze FEATURE=CTAuthentication

# Find related patterns in codebase
./bin/serena-ios-dev.sh analyze Authentication
```

### 2. Automated Generation

```bash
# Generate complete 6-layer implementation
make serena-usecase NAME=CreateUser ENDPOINT=/api/users INPUT=CreateUserRequest OUTPUT=User

# Serena will automatically create:
# - NetworkHelper endpoint definition
# - Target conforming to Requestable
# - Service method implementation
# - Repository protocol and implementation
# - UseCase with CTActionUseCaseType
# - ViewModel execution method
# - Comprehensive unit tests
```

### 3. Integration & Validation

```bash
# Check architecture compliance
make serena-check-arch

# Generate test structure if needed
make serena-generate-tests CLASS=CreateUserViewModel
```

## Requirements (Enhanced with Serena)

### Core Implementation

- **6-Layer Architecture**: NetworkHelper → Targets → Services → Repositories → UseCases → ViewModels
- **Input/Output Models**: Type-safe domain entities in Domain layer
- **Repository Abstraction**: Protocol-based DI with Swinject
- **RxSwift Integration**: Observable streams with proper error handling
- **CTDesignSystem**: Use DS* components throughout UI layer
- **SnapKit Layout**: Mandatory constraint management

### Testing & Quality

- **Unit Tests**: Quick/Nimble with Given-When-Then pattern
- **Mock Generation**: Automatic mock creation for dependencies
- **Architecture Compliance**: Automatic pattern validation
- **Code Coverage**: Minimum 80% target with automated checks

### Serena-Enhanced Features

- **Semantic Analysis**: Understands existing patterns automatically
- **Pattern Recognition**: Identifies similar implementations across modules
- **Refactoring Support**: Safe architectural transformations
- **Code Generation**: Follows established templates and conventions
- **Quality Assurance**: Automated compliance checking

## Serena Integration Benefits

### Productivity Improvements

- **80% Reduction**: In boilerplate code generation
- **Pattern Consistency**: Automatic adherence to project conventions
- **Error Prevention**: Early detection of architectural violations
- **Time Savings**: Complete use case implementation in minutes

### Quality Enhancements

- **Architecture Compliance**: Automatic MVVM pattern enforcement
- **Code Standards**: Consistent with .ruler/ guidelines
- **Testing Coverage**: Automated test structure generation
- **Documentation**: Auto-generated implementation guides

## Deliverables (Serena-Enhanced)

### 1. Complete Implementation

- **Network Layer**: API endpoint definitions and request handling
- **Service Layer**: Concrete implementations with error handling
- **Repository Layer**: Abstraction with protocol-based design
- **Use Case Layer**: Business logic with CTActionUseCaseType
- **ViewModel Layer**: UI presentation logic with RxSwift integration
- **Test Layer**: Comprehensive unit tests with mocks

### 2. Quality Assurance

- **Architecture Validation**: Automatic pattern compliance checks
- **Code Analysis**: Complexity and quality metrics
- **Import Organization**: Consistent import ordering
- **Documentation**: Auto-generated usage examples

### 3. Integration Ready

- **Dependency Injection**: Pre-configured Swinject setup
- **Error Handling**: User-friendly error messages and recovery
- **Logging**: Proper technical logging for debugging
- **Accessibility**: Screen reader support and keyboard navigation

## Usage Example

```bash
# Serena-powered use case generation
make serena-usecase \
  NAME=UpdateUserProfile \
  ENDPOINT=/api/user/profile \
  INPUT=UpdateProfileRequest \
  OUTPUT=UserProfile

# Result: Complete implementation across all 6 layers
# with tests, error handling, and documentation
```

## References

- **Architecture**: .ruler/ct-ai-rule-core-architecture.md
- **Code Standards**: .ruler/ct-ai-rule-code-standards.md
- **Testing**: .ruler/ct-ai-rule-testing-general.md
- **Serena Integration**: SERENA_INTEGRATION.md
- **Project Context**: .serena/ios_context.md

---

*This enhanced workflow leverages Serena's semantic understanding to deliver production-ready use cases that perfectly align with your established architecture and development standards.*
