---
name: ct-unittest
description: Generate C#/.NET unit test structure using xUnit + FluentAssertions + Moq. Creates test class files with Arrange/Act/Assert structure, mock repository, mock navigation service, and mock use case. Use when writing tests for ViewModels, UseCases, or Repositories. Follows Given-When-Then pattern.
---

# WPF Unit Test Generator

Generate unit test structure with xUnit, FluentAssertions, and Moq.

## Input Format

```
CLASS_NAME: <Class being tested, e.g. "UserProfileViewModel">
FEATURE: <Feature module, e.g. "Features/UserManagement">
TEST_TYPE: <viewModel | useCase | repository>
```

## ViewModel Test Template

```csharp
// [ClassName]Tests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace App.[Feature].Tests.ViewModels;

public sealed class [ClassName]Tests
{
    // #region Setup

    private readonly Mock<I[Repository]Repository> _repositoryMock = new();
    private readonly Mock<I[Name]UseCase> _useCaseMock = new();
    // private readonly Mock<I[Name]NavigationService> _navigationMock = new();

    private [ClassName] CreateSut() => new(
        _useCaseMock.Object,
        NullLogger<[ClassName]>.Instance
        // _navigationMock.Object
    );

    // #region Tests — Initialization

    [Fact]
    public void Constructor_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var sut = CreateSut();

        // Assert
        sut.IsLoading.Should().BeFalse();
        sut.ErrorMessage.Should().BeNull();
        sut.Items.Should().BeEmpty();
    }

    // #region Tests — Commands

    [Fact]
    public async Task LoadCommand_ShouldSetIsLoading_ThenFalseAfterCompletion()
    {
        // Arrange
        _useCaseMock.Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.IsLoading.Should().BeFalse();
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadCommand_ShouldPopulateItems_WhenUseCaseSucceeds()
    {
        // Arrange
        var expected = new List<[Model]>
        {
            new() { Id = "1", Name = "Item A" },
            new() { Id = "2", Name = "Item B" }
        };
        _useCaseMock.Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.Items.Should().HaveCount(2);
        sut.Items[0].Name.Should().Be("Item A");
    }

    [Fact]
    public async Task LoadCommand_ShouldSetErrorMessage_WhenUseCaseThrows()
    {
        // Arrange
        _useCaseMock.Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.ErrorMessage.Should().NotBeNullOrEmpty();
        sut.IsLoading.Should().BeFalse();
    }
}
```

## UseCase Test Template

```csharp
// [UseCaseName]Tests.cs
using FluentAssertions;
using Moq;
using Xunit;

namespace App.[Feature].Tests.Domain;

public sealed class [UseCaseName]Tests
{
    private readonly Mock<I[Name]Repository> _repositoryMock = new();

    private [UseCaseName] CreateSut() => new(_repositoryMock.Object);

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMappedResult_WhenRepositorySucceeds()
    {
        // Arrange
        var data = new List<[Model]> { new() { Id = "1", Name = "Test" } };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagate_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        var sut = CreateSut();

        // Act
        var act = async () => await sut.ExecuteAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");
    }
}
```

## Repository Test Template

```csharp
// [Name]RepositoryTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace App.[Feature].Tests.Data;

public sealed class [Name]RepositoryTests
{
    private readonly Mock<I[Name]Service> _serviceMock = new();

    private [Name]Repository CreateSut()
        => new(_serviceMock.Object, NullLogger<[Name]Repository>.Instance);

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedEntity_WhenServiceReturnsDto()
    {
        // Arrange
        var dto = new [Entity]Dto { Id = "1", Name = "Test Entity" };
        _serviceMock.Setup(s => s.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync("1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
        result.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenServiceReturnsNull()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityDto?)null);
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }
}
```

## Test Organization Patterns

### Theory (Parameterized Tests)

```csharp
[Theory]
[InlineData("", false)]
[InlineData("  ", false)]
[InlineData("valid@email.com", true)]
public void ValidateEmail_ShouldReturn_ExpectedResult(string email, bool expected)
{
    var result = EmailValidator.Validate(email);
    result.Should().Be(expected);
}
```

### BDD-Style Naming Convention

```
[MethodName]_Should[ExpectedBehavior]_When[Condition]

Examples:
- LoadCommand_ShouldSetIsLoading_WhenExecuted
- GetByIdAsync_ShouldReturnNull_WhenIdNotFound
- ExecuteAsync_ShouldThrow_WhenRepositoryFails
```

## Required NuGet Packages

```xml
<!-- In test .csproj -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.*" />
```

## Run Tests

```bash
dotnet test
dotnet test --filter "ClassName=UserProfileViewModelTests"
dotnet test --collect:"XPlat Code Coverage"
```

## Input Format

```
CLASS_NAME: <Class being tested, e.g. "UserProfileViewModel">
FEATURE: <Feature module, e.g. "CTUserManagement">
TEST_TYPE: <viewModel | useCase | repository>
```

## ViewModel Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using [FeatureModule].ViewModels;
using [FeatureModule].Repositories;

namespace [FeatureModule].Tests.ViewModels;

public class [ClassName]ViewModelTests
{
    private readonly Mock<I[Repository]> _repositoryMock = new();
    private readonly Mock<ILogger<[ClassName]ViewModel>> _loggerMock = new();

    private [ClassName]ViewModel CreateSut() =>
        new(_repositoryMock.Object, _loggerMock.Object);

    [Fact]
    public void Constructor_ShouldInitialize_WithDefaults()
    {
        var sut = CreateSut();

        sut.IsLoading.Should().BeFalse();
        sut.Items.Should().BeEmpty();
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadCommand_ShouldSetIsLoading_WhenExecuted()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemDto>());
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.IsLoading.Should().BeFalse();
        _repositoryMock.Verify(r => r.GetListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadCommand_ShouldSetErrorMessage_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.ErrorMessage.Should().NotBeNull();
        sut.IsLoading.Should().BeFalse();
    }
}
```

## Mock Repository Template

```csharp
using Moq;
using [FeatureModule].Repositories;

// Auto-mocked via Moq — no manual class needed:
var repositoryMock = new Mock<I[RepositoryName]>();

repositoryMock
    .Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<ItemDto> { /* seed data */ });

repositoryMock
    .Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ItemDto { Id = "123", Name = "Test" });

// Verify calls
repositoryMock.Verify(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()), Times.Once);
```

## Mock Service Template

```csharp
using Moq;
using [FeatureModule].Services;

var serviceMock = new Mock<I[ServiceName]>();

serviceMock
    .Setup(s => s.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ResponseDto { /* data */ });

// Throw scenario
serviceMock
    .Setup(s => s.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ThrowsAsync(new HttpRequestException("Service unavailable"));
```

## Test Organization Patterns

### Theory (Parameterized Tests)

```csharp
[Theory]
[InlineData("", false)]
[InlineData("  ", false)]
[InlineData("valid@email.com", true)]
public void ValidateEmail_ShouldReturn_ExpectedResult(string email, bool expected)
{
    var result = EmailValidator.Validate(email);
    result.Should().Be(expected);
}
```

### Given-When-Then

```csharp
[Fact]
public async Task LoadCommand_ShouldPopulateItems_WhenServiceSucceeds()
{
    // Given
    var expected = new List<ItemDto> { new() { Id = "1", Name = "Item" } };
    _repositoryMock.Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);
    var sut = CreateSut();

    // When
    await sut.LoadCommand.ExecuteAsync(null);

    // Then
    sut.Items.Should().HaveCount(1);
    sut.Items.First().Name.Should().Be("Item");
    sut.IsLoading.Should().BeFalse();
}
```

### Mock Verification

```csharp
[Fact]
public async Task LoadUser_ShouldCallRepository_WithCorrectId()
{
    // Given
    var userId = "123";
    _repositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new UserDto { Id = userId });
    var sut = CreateSut();

    // When
    await sut.LoadUserCommand.ExecuteAsync(userId);

    // Then
    _repositoryMock.Verify(
        r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## Rules

- All test classes are `public` with xUnit `[Fact]` / `[Theory]` attributes
- Always create mocks as private fields, instantiate SUT in a `CreateSut()` factory
- Use Moq `.Setup()` for stubs and `.Verify()` for interaction assertions
- Use FluentAssertions `.Should().Be()`, `.Should().BeNull()`, `.Should().HaveCount()`
- Never test implementation details — test observable behavior only
- Tests must be independent and order-agnostic
- Naming convention: `[Method]_Should[Behavior]_When[Condition]`
