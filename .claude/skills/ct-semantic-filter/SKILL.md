---
name: ct-semantic-filter
description: Semantically filter and clean feature requirement content for Desktop Lamour development. Removes sensitive business data (revenue targets, internal metrics, competitor info) while preserving all technical requirements (API specs, user stories, UI/UX flows, validation rules, business rules). Use BEFORE passing requirements to any other skill.
model: haiku
effort: low
---

# Semantic Filter — Requirements Cleaner for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Filters raw feature requirement content to remove sensitive business information while preserving everything needed for MVVM + Clean Architecture implementation.

---

## Input Format

```
RAW REQUIREMENTS:
"""
[Paste your complete, unfiltered requirements here]
"""
```

---

## Filter Rules

### REMOVE or ANONYMIZE

| Category | Examples |
|---|---|
| Sensitive business data | Revenue targets, GMV, conversion rates, user counts, growth metrics |
| Internal team info | Employee names, team names, org structure, stakeholder lists |
| Competitive intel | Competitor names, market share, benchmark comparisons |
| Financial details | Budget, cost breakdowns, pricing strategy, margins |
| Legal/compliance specifics | Regulatory opinions, legal entity names, law references |
| Internal processes | Approval workflows, release schedules, sprint plans |

### PRESERVE

| Category | Examples |
|---|---|
| User stories | "As a user, I want to..." |
| API specifications | Endpoints, HTTP methods, request/response schemas |
| Functional requirements | Feature behaviors, system responses, validation rules |
| UI/UX specifications | Screen layouts, component requirements, navigation flows |
| Error handling | Error states, fallback behaviors, edge cases |
| Business rules | Stock validation, invoice immutability, role-based access |
| Performance requirements | Response time SLAs, pagination sizes |
| Security specifications | Auth flows, data validation requirements |

---

## Desktop Lamour Preservation Focus

When filtering, always keep intact:

### MVVM + Clean Architecture
- ViewModel state requirements (loading, error, empty states)
- Data binding requirements between layers
- Domain model field definitions (name, type, required/optional)
- UseCase business logic descriptions
- Business rule constraints (stock never negative, invoice immutable after confirmation)

### WPF UI Requirements
- Window type (dialog vs main window vs UserControl)
- Form field list with types (TextBox, ComboBox, DatePicker)
- DataGrid column specifications
- Button actions and their trigger conditions
- Validation rules per field

### API & Data Layer
- Endpoint paths and HTTP methods
- Request body field names and types
- Response schema with field names and types
- Pagination, sorting, filtering specs
- Error response shapes (400/404/409 bodies)

### Business Domain Rules (Desktop Lamour)
- Employee role constraints: Admin / Thu ngân / Kho
- Inventory: stock quantity, unit of measure, product category
- ImportInvoice: supplier, line items, total, confirmation status
- ExportInvoice: customer, line items, stock deduction rules

---

## Output Format

```
FILTERED REQUIREMENTS — [Feature Name]

## Summary
[1–2 sentence description, business context anonymized]

## User Stories
[Preserved user-facing requirements]

## Functional Requirements
[All technical behaviors, system responses, edge cases]

## UI/UX Specifications
[Screen layout, form fields, DataGrid columns, button actions]

## API Specifications
[Endpoints, request/response schemas, error shapes]

## Validation & Business Rules
[Field validation rules, domain constraints, error states]

## Module & Layer Impact
[Which Desktop Lamour module, which layers are affected]

---
Removed: [brief list — e.g., "Revenue targets, team names"]
Preserved: [brief list — e.g., "All API specs, validation rules, UI states"]
```

---

## Example

**Input:**
```
RAW REQUIREMENTS:
"""
# Inventory Restock Feature — Q2 Cost Reduction Initiative

## Business Context
The ops team (Nguyen Van A, Tran Thi B) found that 40% of stockouts cost us 500M VND/month.
Competitor analysis shows Shopify achieves 95% stock accuracy. We need to cut waste by 30%.

## Technical Requirements
- Store managers can create import invoices with multiple product line items
- Each line item has: product_id, quantity (int, min 1), unit_price (decimal)
- POST /api/import-invoices creates the invoice and updates stock
- GET /api/import-invoices returns paginated list (20 per page)
- Confirmed invoices cannot be edited or deleted
- Stock quantity increases when invoice is confirmed

## UI Requirements
- DataGrid showing invoice list with columns: ID, date, supplier, total, status
- "Tạo phiếu nhập" button opens a modal dialog
- Modal has supplier name field, line items table (add/remove rows), confirm button
- Status badge: Chờ xác nhận (yellow) / Đã xác nhận (green)
"""
```

**Output:**
```
FILTERED REQUIREMENTS — Import Invoice Feature

## Summary
Store managers need to record inventory import invoices with multiple product line items,
updating stock quantities upon confirmation.

## User Stories
- As a store manager, I want to create import invoices with multiple product line items
- As a store manager, I want to confirm an invoice to trigger stock updates
- As a store manager, I want to view a paginated list of all import invoices

## Functional Requirements
- Each line item contains: product_id (int), quantity (int, min 1), unit_price (decimal)
- Confirming an invoice increases product stock quantities for each line item
- Confirmed invoices cannot be edited or deleted (immutable)
- Invoice list is paginated: 20 items per page

## UI/UX Specifications
- DataGrid columns: ID, Date, Supplier, Total, Status
- "Tạo phiếu nhập" button opens CreateImportInvoiceWindow (modal dialog)
- Modal form: supplier name (TextBox), line items DataGrid (add/remove rows), confirm button
- Status display: "Chờ xác nhận" / "Đã xác nhận" with DataTrigger color change

## API Specifications
- POST /api/import-invoices — create invoice (request: supplier_name, line_items[])
- GET /api/import-invoices — paginated list (page, page_size=20)
- Response: { id, created_at, supplier_name, total, status, line_items[] }

## Validation & Business Rules
- quantity: int, minimum 1
- unit_price: decimal, greater than 0
- Confirmed invoices: read-only, no edit/delete allowed
- Stock quantity increases on confirmation (never decreases below 0)

## Module & Layer Impact
- Module: ImportInvoices
- Domain: ImportInvoice model, CreateImportInvoiceUseCase, ConfirmImportInvoiceUseCase
- Data: IImportInvoiceRepository, IImportInvoiceService, ImportInvoiceDto
- Presentation: ImportInvoicesViewModel, ImportInvoicesView, CreateImportInvoiceWindow

---
Removed: Revenue targets (500M VND/month), team names (Nguyen Van A, Tran Thi B), competitor benchmark (Shopify 95%)
Preserved: All API specs, validation rules, UI layout, business rules (immutability, stock update)
```

---

## Recommended Workflow

Use this skill before passing requirements to other skills:

```
1. /ct-semantic-filter        — strip confidential data first
2. /ct-flipped-interaction    — clarify technical gaps
3. /ct-generate-usecase       — generate UseCase from clean requirements
4. /ct-quality-engineer       — validate implementation against clean spec
5. /ct-figma-implement-design — implement UI from clean specs
```

---

## Notes

- If no sensitive data is found, output the requirements unchanged with: `No sensitive data detected — requirements passed through unchanged.`
- If content is ambiguous (could be sensitive or technical), preserve it and flag: `Flagged for review: [field] — may contain sensitive data.`
- Never invent or hallucinate technical requirements. Only filter; never add.

See `docs/project-overview.md` for business domain context.
