---
description: "Generate and wire a new UseCase across all 5 layers of Desktop Lamour."
mode: "agent"
---

# UseCase Generator — Desktop Lamour

Wire a new UseCase end-to-end: Domain → Data → Presentation → DI.

## Input

```
USECASE_NAME:  <e.g. GetEmployees>
MODULE:        <Employees>
HTTP_METHOD:   <GET | POST | PUT | DELETE>
ENDPOINT:      </api/employees>
INPUT_TYPE:    <GetEmployeesRequest>
OUTPUT_TYPE:   <PagedResult<Employee>>
```

## Generation Checklist

- [ ] IUseCase + UseCase in Domain/UseCases/
- [ ] IRepository + Repository in Data/Repositories/
- [ ] IService + Service in Data/Services/
- [ ] RequestDto + ResponseDto in Data/Services/Dtos/
- [ ] ViewModel [RelayCommand] with IsLoading, ErrorMessage, try/catch/finally
- [ ] DI registration in [Module]ServiceCollectionExtensions.cs

## Rules

- Domain has ZERO external dependencies
- All cross-layer communication through interfaces
- No `new` for Services/Repositories — always DI
- CancellationToken ct = default on every async method
- Never .Result or .Wait()
