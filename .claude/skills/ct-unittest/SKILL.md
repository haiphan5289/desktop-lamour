---
name: ct-unittest
description: Generate xUnit unit tests for Desktop Lamour C# classes. Creates test file with [Fact]/[Theory] tests, Moq mocks for interfaces, Arrange/Act/Assert pattern. Use for ViewModel, UseCase, or Repository tests covering happy path, error path, and edge cases.
model: sonnet
effort: medium
---

# xUnit Unit Test Generator for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate xUnit unit tests using Moq for Desktop Lamour C# classes.

## Input Format

```
CLASS_NAME: <class being tested, e.g. "GetEmployeesUseCase">
CLASS_TYPE: <ViewModel | UseCase | Repository>
MODULE: <e.g. "Employees">
```

---

## ViewModel Test Template

```csharp
// File: tests/DesktopLamour.Tests/Features/[Module]/[Name]ViewModelTests.cs
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Features.[Module].Domain;
using DesktopLamour.Features.[Module].ViewModels;
using Moq;
using Xunit;

namespace DesktopLamour.Tests.Features.[Module];

public class [Name]ViewModelTests
{
    private readonly Mock<IGet[Entity]sUseCase> _mockUseCase;
    private readonly [Name]ViewModel _sut;

    public [Name]ViewModelTests()
    {
        _mockUseCase = new Mock<IGet[Entity]sUseCase>();
        _sut = new [Name]ViewModel(_mockUseCase.Object);
    }

    [Fact]
    public async Task Load[Entity]sCommand_OnSuccess_PopulatesCollection()
    {
        // Arrange
        var expected = new List<[Entity]>
        {
            new() { Id = 1, FullName = "Test Employee" }
        };
        _mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        await _sut.Load[Entity]sCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_sut.Items);
        Assert.Equal("Test Employee", _sut.Items[0].FullName);
        Assert.False(_sut.IsLoading);
        Assert.Empty(_sut.ErrorMessage);
    }

    [Fact]
    public async Task Load[Entity]sCommand_OnException_SetsErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _sut.Load[Entity]sCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(_sut.Items);
        Assert.NotEmpty(_sut.ErrorMessage);
        Assert.False(_sut.IsLoading); // IsLoading must be false even after error
    }

    [Fact]
    public async Task Load[Entity]sCommand_OnEmptyResult_CollectionIsEmpty()
    {
        // Arrange
        _mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<[Entity]>());

        // Act
        await _sut.Load[Entity]sCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(_sut.Items);
        Assert.False(_sut.IsLoading);
    }

    [Fact]
    public async Task Load[Entity]sCommand_Always_SetsIsLoadingFalseInFinally()
    {
        // Arrange — use task that throws
        _mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Any error"));

        // Act
        await _sut.Load[Entity]sCommand.ExecuteAsync(null);

        // Assert
        Assert.False(_sut.IsLoading); // Must be false even on exception
    }
}
```

---

## UseCase Test Template

```csharp
// File: tests/DesktopLamour.Tests/Features/[Module]/[Name]UseCaseTests.cs
using DesktopLamour.Features.[Module].Data;
using DesktopLamour.Features.[Module].Domain;
using Moq;
using Xunit;

namespace DesktopLamour.Tests.Features.[Module];

public class [Name]UseCaseTests
{
    private readonly Mock<I[Entity]Repository> _mockRepository;
    private readonly [Name]UseCase _sut;

    public [Name]UseCaseTests()
    {
        _mockRepository = new Mock<I[Entity]Repository>();
        _sut = new [Name]UseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRepositoryResult()
    {
        // Arrange
        var expected = new List<[Entity]>
        {
            new() { Id = 1, FullName = "Employee 1" }
        };
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        Assert.Equal(expected, result);
        _mockRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ExecuteAsync());
    }

    // Business rule test (e.g. for ExportInvoice UseCase):
    [Fact]
    public async Task ExecuteAsync_WhenStockInsufficient_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = new Product { Id = 1, Stock = 5 };
        _mockRepository
            .Setup(x => x.GetProductAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new ExportInvoiceLine { ProductId = 1, Quantity = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ExecuteAsync(request));
    }
}
```

---

## Repository Test Template

```csharp
// File: tests/DesktopLamour.Tests/Features/[Module]/[Name]RepositoryTests.cs
using DesktopLamour.Features.[Module].Data;
using DesktopLamour.Features.[Module].Data.DTOs;
using Moq;
using Xunit;

namespace DesktopLamour.Tests.Features.[Module];

public class [Name]RepositoryTests
{
    private readonly Mock<I[Entity]Service> _mockService;
    private readonly [Name]Repository _sut;

    public [Name]RepositoryTests()
    {
        _mockService = new Mock<I[Entity]Service>();
        _sut = new [Name]Repository(_mockService.Object);
    }

    [Fact]
    public async Task GetAllAsync_MapssDtoToDomainModel()
    {
        // Arrange
        var dtos = new List<[Entity]Dto>
        {
            new() { Id = 1, FullName = "Test", PhoneNumber = "0901234567" }
        };
        _mockService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(1, single.Id);
        Assert.Equal("Test", single.FullName);
        Assert.Equal("0901234567", single.PhoneNumber);
    }

    [Fact]
    public async Task GetAllAsync_WhenServiceReturnsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<[Entity]Dto>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }
}
```

---

## [Theory] with Test Data

```csharp
[Theory]
[InlineData(0, 5, false)]   // requested 0, stock 5 — should be valid
[InlineData(5, 5, true)]    // requested 5, stock 5 — exact match, valid
[InlineData(6, 5, false)]   // requested 6, stock 5 — invalid
public async Task ExportUseCase_StockValidation(
    int requested, int available, bool expectException)
{
    // Arrange
    var product = new Product { Stock = available };
    _mockRepository.Setup(x => x.GetProductAsync(1, default)).ReturnsAsync(product);

    var line = new ExportInvoiceLine { ProductId = 1, Quantity = requested };

    // Act & Assert
    if (expectException)
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(line));
    else
        await _sut.ExecuteAsync(line); // Should not throw
}
```

---

## Test Project Setup

```xml
<!-- tests/DesktopLamour.Tests/DesktopLamour.Tests.csproj -->
<ItemGroup>
    <PackageReference Include="xunit" Version="2.9.*"/>
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.*"/>
    <PackageReference Include="Moq" Version="4.20.*"/>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.*"/>
</ItemGroup>
```

---

## Rules

- Test class name: `[ClassName]Tests`
- Constructor creates SUT (`_sut`) and mocks
- Use Moq `Mock<IInterface>` for all dependencies
- `[Fact]` for single-path tests, `[Theory]` with `[InlineData]` for parameterized tests
- Arrange / Act / Assert comments in each test method
- Verify `IsLoading = false` after any async command (even on error)
- Business rule tests use `Assert.ThrowsAsync<>` for validation failures
- Never test implementation details — test observable behavior only
