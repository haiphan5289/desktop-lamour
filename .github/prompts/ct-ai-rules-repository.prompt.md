---
description: "Generate a Repository (interface + implementation) for Desktop Lamour."
mode: "agent"
---

# Repository Generator — Desktop Lamour

## Input

```
MODULE:      <Employees>
NAME:        <Employee>
SERVICE:     <IEmployeeService>
ACTION:      <GetList | GetById | Create | Update | Delete>
INPUT_TYPE:  <GetEmployeesRequest>
OUTPUT_TYPE: <IEnumerable<Employee>>
```

## Template

```csharp
// Data/Repositories/I[Name]Repository.cs
public interface I[Name]Repository
{
    Task<[OutputType]> [Action]Async([InputType] input, CancellationToken ct = default);
}

// Data/Repositories/[Name]Repository.cs
public class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;

    public [Name]Repository(I[Name]Service service) => _service = service;

    public async Task<[OutputType]> [Action]Async([InputType] input, CancellationToken ct = default)
    {
        var dto = await _service.[Action]Async(
            new [RequestDto] { /* map fields */ }, ct);
        return dto?.Items?.Select(x => new [Name] { Id = x.Id }) ?? [];
    }
}
```
