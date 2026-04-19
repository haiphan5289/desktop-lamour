---
name: ct-flipped-interaction
description: Ask clarifying questions before implementing any Desktop Lamour feature. Use when a feature request is vague — gather module scope, API contracts, business rules (stock validation, VAT, role permissions), XAML layout expectations, and error handling requirements.
model: haiku
effort: low
---

# Flipped Interaction — Clarify Before Implementing

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

Before writing any C# or XAML code for Desktop Lamour, ask the user a focused set of clarifying questions. This prevents wasted effort from wrong assumptions about module scope, API contracts, business rules, or layout expectations.

## When to Use

- Feature request is described in 1–2 sentences with no technical detail
- Module is not specified (Authentication / Employees / Inventory / ImportInvoices / ExportInvoices)
- API endpoint is unknown
- Business rules around stock, pricing, or roles are unclear
- UI layout is not described

## Clarifying Questions Template

Ask ALL relevant questions as a single grouped message. Do NOT ask one question at a time.

---

### Group 1 — Scope

1. Which module does this feature belong to?
   - Authentication | Employees | Inventory | ImportInvoices | ExportInvoices

2. Which layer(s) need to change?
   - Domain (UseCase/Model) | Data (Service/Repository/DTO) | Presentation (ViewModel/View) | All layers

3. Is this a new feature or modifying an existing one? If existing, which class/file?

---

### Group 2 — API Contract

4. Is the API endpoint already defined? If yes, what is the path and HTTP method?
   - Example: `GET /api/employees`, `POST /api/invoices/export`

5. What does the request body / query parameters look like?

6. What does the response JSON look like? Provide a sample if possible.

---

### Group 3 — Business Rules

7. Are there any validation rules?
   - Stock must be ≥ requested quantity?
   - Invoice total must be recalculated on line item change?
   - Phone number must be unique for authentication?

8. Which user roles can perform this action?
   - Admin | Thu ngân (Cashier) | Kho (Warehouse)

9. Are there any immutability rules?
   - Example: invoices cannot be modified after confirmation

10. Is there a stock deduction / stock addition side effect?

---

### Group 4 — UI & ViewModel

11. What does the UI need to show?
    - List / DataGrid of items?
    - Form with input fields?
    - Modal dialog?
    - Loading overlay while saving?

12. What should happen on success? What on error?
    - Navigate back | Show success message | Refresh list | Stay on form

13. Are there any specific XAML layout requirements?
    - Fixed window size? Resizable? Dialog vs page?

---

### Group 5 — Testing

14. Should unit tests be generated alongside the implementation?
    - ViewModel only | UseCase only | Repository only | All layers

15. Are there known edge cases to cover in tests?

---

## Output After Clarification

Once the user answers, summarize in this format before implementing:

```
MODULE:          <Employees>
LAYERS:          <Domain + Data + Presentation>
API_ENDPOINT:    <GET /api/employees>
HTTP_METHOD:     <GET>
INPUT_TYPE:      <GetEmployeesRequest (page, pageSize)>
OUTPUT_TYPE:     <IEnumerable<Employee>>
BUSINESS_RULES:  <Admin only; active employees only>
UI_LAYOUT:       <DataGrid with pagination, search bar>
ERROR_HANDLING:  <Show ErrorMessage label, IsLoading = false>
TESTS_NEEDED:    <Yes — UseCase + ViewModel>
```

Then proceed to implementation following Clean Architecture: Domain → Data → Presentation → DI registration.

See `docs/project-overview.md` for full project context.
