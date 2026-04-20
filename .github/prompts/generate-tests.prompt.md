---
description: "Generate xUnit unit tests for any C# class in Desktop Lamour."
mode: "agent"
---

# Quick Test Generator — Desktop Lamour

Generate xUnit + Moq tests for a C# class.

## Input

```
CLASS_FILE: <path to the .cs file to test>
CLASS_NAME: <[Name]ViewModel | [Name]UseCase | etc.>
```

## Rules

- Use xUnit `[Fact]` and `[Theory]`
- Use Moq for all interface dependencies
- Follow Arrange / Act / Assert
- Test file goes in `tests/DesktopLamour.Tests/Features/[Module]/`
- Cover: happy path, exception path, IsLoading=false in finally, empty input validation
- For ViewModels: test that `[RelayCommand]` methods correctly handle errors and reset IsLoading
- For UseCases: test business rules (stock >= 0, invoice immutability)
