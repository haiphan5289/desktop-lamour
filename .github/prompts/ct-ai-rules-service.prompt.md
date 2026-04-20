---
description: "Generate a Service (interface + implementation) for Desktop Lamour."
mode: "agent"
---

# Service Generator — Desktop Lamour

## Input

```
MODULE:       <Employees>
NAME:         <Employee>
HTTP_METHOD:  <GET | POST | PUT | DELETE>
ENDPOINT:     </api/employees>
REQUEST_DTO:  <GetEmployeesRequestDto>
RESPONSE_DTO: <GetEmployeesResponseDto>
```

## Template

```csharp
// Data/Services/I[Name]Service.cs
public interface I[Name]Service
{
    Task<[ResponseDto]?> [Action]Async([RequestDto] request, CancellationToken ct = default);
}

// Data/Services/[Name]Service.cs
public class [Name]Service : I[Name]Service
{
    private readonly HttpClient _http;
    public [Name]Service(HttpClient http) => _http = http;

    public async Task<[ResponseDto]?> [Action]Async([RequestDto] request, CancellationToken ct = default)
    {
        // GET: return await _http.GetFromJsonAsync<[ResponseDto]>($"/api/endpoint?page={request.Page}", ct);
        // POST:
        var response = await _http.PostAsJsonAsync("/api/endpoint", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<[ResponseDto]>(cancellationToken: ct);
    }
}

// Dtos/[Name]RequestDto.cs
public record [Name]RequestDto(int Page, int PageSize);

// Dtos/[Name]ResponseDto.cs
public record [Name]ResponseDto(IEnumerable<[Name]ItemDto> Items, int Total);
public record [Name]ItemDto(int Id /* TODO: add fields */);
```
