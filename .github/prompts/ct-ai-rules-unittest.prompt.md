---
description: "Generate C#/.NET unit test structure using xUnit + FluentAssertions + Moq"
mode: "agent"
---

# WPF Unit Test Generator

Generate unit test structure following xUnit + FluentAssertions + Moq patterns.

## Instructions

Reference our C#/.NET WPF development guidelines: [WPF Guidelines](../instructions/wpf-general-instructions.instructions.md)

Generate unit test structure with:

-   xUnit testing framework (`[Fact]`, `[Theory]`)
-   Moq for mocking (`Mock<T>`, `.Setup()`, `.Verify()`)
-   FluentAssertions for assertions (`.Should().Be()`)
-   Given-When-Then test organization
-   Private `CreateSut()` factory pattern

## Required NuGet Packages

```xml
<!-- In test .csproj -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.*" />
```

## ViewModel Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using [FeatureModule].ViewModels;
using [FeatureModule].Repositories;
using [FeatureModule].Domain.Models;

namespace [FeatureModule].Tests.ViewModels;

public class [ClassName]ViewModelTests
{
    private readonly Mock<I[Repository]> _repositoryMock = new();
    private readonly NullLogger<[ClassName]ViewModel> _logger = new();

    private [ClassName]ViewModel CreateSut() =>
        new(_repositoryMock.Object, _logger);

    [Fact]
    public void Constructor_ShouldInitialize_WithDefaults()
    {
        var sut = CreateSut();

        sut.IsLoading.Should().BeFalse();
        sut.Items.Should().BeEmpty();
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadCommand_ShouldPopulateItems_WhenRepositorySucceeds()
    {
        // Arrange
        var expected = new List<ItemDto> { new() { Id = "1", Name = "Test Item" } };
        _repositoryMock
            .Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.Items.Should().HaveCount(1);
        sut.Items.First().Name.Should().Be("Test Item");
        sut.IsLoading.Should().BeFalse();
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadCommand_ShouldSetErrorMessage_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.ErrorMessage.Should().NotBeNull();
        sut.IsLoading.Should().BeFalse();
        sut.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadCommand_ShouldCallRepository_Once()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemDto>());
        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        _repositoryMock.Verify(r => r.GetListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

## Repository Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using [FeatureModule].Data.Repositories;
using [FeatureModule].Data.Services;
using [FeatureModule].Domain.Models;

namespace [FeatureModule].Tests.Repositories;

public class [Name]RepositoryTests
{
    private readonly Mock<I[Name]Service> _serviceMock = new();

    private [Name]Repository CreateSut() =>
        new(_serviceMock.Object);

    [Fact]
    public async Task GetListAsync_ShouldReturnItems_WhenServiceSucceeds()
    {
        // Arrange
        var items = new List<ItemDto> { new() { Id = "1" } };
        _serviceMock
            .Setup(s => s.FetchListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var sut = CreateSut();

        // Act
        var result = await sut.GetListAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnEmpty_WhenServiceReturnsNull()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.FetchListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ItemDto>?)null);
        var sut = CreateSut();

        // Act
        var result = await sut.GetListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.FetchByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemDto?)null);
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync("missing");

        // Assert
        result.Should().BeNull();
    }
}
```

## UseCase Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using [FeatureModule].Domain.UseCases;
using [FeatureModule].Data.Repositories;
using [FeatureModule].Domain.Models;

namespace [FeatureModule].Tests.UseCases;

public class [Name]UseCaseTests
{
    private readonly Mock<I[Name]Repository> _repositoryMock = new();

    private [Name]UseCase CreateSut() =>
        new(_repositoryMock.Object);

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResult_WhenRepositorySucceeds()
    {
        // Arrange
        var input = new [InputType] { Id = "123" };
        var expected = new [OutputType] { Id = "123", Name = "Test" };
        _repositoryMock
            .Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(input);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
```

## Mock Patterns

```csharp
// Mock with ReturnsAsync
_mock.Setup(x => x.MethodAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
     .ReturnsAsync(expectedValue);

// Mock throwing
_mock.Setup(x => x.MethodAsync(It.IsAny<CancellationToken>()))
     .ThrowsAsync(new HttpRequestException("error"));

// Verify call count
_mock.Verify(x => x.MethodAsync(It.IsAny<CancellationToken>()), Times.Once);
_mock.Verify(x => x.MethodAsync(It.IsAny<CancellationToken>()), Times.Never);

// Verify with exact parameter
_mock.Verify(x => x.GetByIdAsync("exact-id", It.IsAny<CancellationToken>()), Times.Once);
```

## Theory (Parameterized Tests)

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

## FluentAssertions Reference

```csharp
// Equality
result.Should().Be(expected);
result.Should().BeEquivalentTo(expected);   // deep/structural equality

// Nullability
result.Should().BeNull();
result.Should().NotBeNull();

// Collections
list.Should().BeEmpty();
list.Should().HaveCount(3);
list.Should().Contain(item => item.Id == "1");
list.Should().ContainSingle(x => x.Name == "Test");

// Strings
str.Should().StartWith("prefix");
str.Should().Contain("substring");

// Booleans
flag.Should().BeTrue();
flag.Should().BeFalse();

// Exceptions
await sut.Invoking(x => x.LoadAsync()).Should().ThrowAsync<InvalidOperationException>();
```

## Test Naming Convention

```
[MethodName]_Should[ExpectedBehavior]_When[Condition]

Examples:
- LoadCommand_ShouldPopulateItems_WhenRepositorySucceeds
- GetByIdAsync_ShouldReturnNull_WhenNotFound
- ExecuteAsync_ShouldThrow_WhenRepositoryFails
- Constructor_ShouldInitialize_WithDefaults
```

## Run Tests

```bash
dotnet test
dotnet test --filter "ClassName=[Name]Tests"
dotnet test --collect:"XPlat Code Coverage"
```

## Rules

- All test classes are `public` with xUnit `[Fact]` / `[Theory]` attributes
- Create mocks as private `readonly` fields, instantiate SUT via `CreateSut()` factory
- Use `.Setup()` for stubs and `.Verify()` for interaction assertions
- Use FluentAssertions — never `Assert.Equal()` directly
- Never test implementation details — test observable behavior
- Tests must be independent and order-agnostic
