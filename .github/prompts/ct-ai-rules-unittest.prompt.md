---
description: "Generate xUnit unit tests for Desktop Lamour ViewModel, UseCase, or Repository."
mode: "agent"
---

# Unit Test Generator — Desktop Lamour

Generate xUnit + Moq tests following Arrange/Act/Assert.

## Input

```
CLASS:     <[Name]ViewModel | [Name]UseCase | [Name]Repository>
MODULE:    <Employees>
SCENARIOS: <happy path, error path, validation edge cases>
```

## ViewModel Test Template

```csharp
using Moq;
using DesktopLamour.Features.[Module].ViewModels;
using DesktopLamour.Features.[Module].Domain.UseCases;

public class [Name]ViewModelTests
{
    private readonly Mock<I[Name]UseCase> _useCaseMock = new();
    private [Name]ViewModel CreateSut() => new(_useCaseMock.Object);

    [Fact]
    public async Task Load_WhenSuccess_SetsItems()
    {
        // Arrange
        var expected = new List<[Model]> { /* test data */ };
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<[Input]>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);
        var sut = CreateSut();

        // Act
        await sut.Load[Name]Command.ExecuteAsync(null);

        // Assert
        Assert.Equal(expected.Count, sut.Items.Count);
        Assert.False(sut.IsLoading);
        Assert.Empty(sut.ErrorMessage);
    }

    [Fact]
    public async Task Load_WhenException_SetsErrorMessage()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<[Input]>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Network error"));
        var sut = CreateSut();

        // Act
        await sut.Load[Name]Command.ExecuteAsync(null);

        // Assert
        Assert.NotEmpty(sut.ErrorMessage);
        Assert.False(sut.IsLoading);
    }
}
```

## UseCase Test Template

```csharp
public class [Name]UseCaseTests
{
    private readonly Mock<I[Name]Repository> _repoMock = new();

    [Fact]
    public async Task Execute_ReturnsRepositoryResult()
    {
        // Arrange
        var expected = /* test data */;
        _repoMock.Setup(x => x.[Action]Async(It.IsAny<[Input]>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(expected);
        var sut = new [Name]UseCase(_repoMock.Object);

        // Act
        var result = await sut.ExecuteAsync(new [Input](), CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
    }
}
```
