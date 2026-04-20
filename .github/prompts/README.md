# Desktop Lamour — AI Prompt Library

Prompt files for VS Code Copilot agent mode. Each file targets a specific development task in the **Desktop Lamour** WPF cosmetics POS application.

## Stack

- .NET 8, WPF, Windows
- CommunityToolkit.Mvvm 8.3.2
- Microsoft.Extensions.DependencyInjection
- Design System: AppButton / AppLabel / AppTextField / AppPasswordField
- Modules: Authentication | Employees | Inventory | ImportInvoices | ExportInvoices

---

## Prompt Index

### Architecture & Analysis

| File | Purpose |
|---|---|
| `ct-ai-persona-pattern.prompt.md` | WPF expert persona — architecture rules and module context |
| `ct-ai-chain-of-thought-pattern.prompt.md` | Step-by-step technical design for complex features |
| `ct-ai-alternative-approaches-pattern.prompt.md` | Generate 3–4 alternative solutions with comparison matrix |
| `ct-ai-flipped-interaction-pattern.prompt.md` | Ask clarifying questions before implementing |
| `ct-ai-git-diff.prompt.md` | Review a git diff against MVVM + business rules |

### Code Generation

| File | Purpose |
|---|---|
| `ct-ai-rules-scaffold.prompt.md` | Barebone ViewModel / UseCase / Repository / Service / View files |
| `ct-ai-rules-module.prompt.md` | Full feature module — all 5 layers + DI |
| `ct-ai-rules-usecase.prompt.md` | Wire a UseCase end-to-end across all layers |
| `ct-ai-rules-repository.prompt.md` | Repository interface + implementation |
| `ct-ai-rules-service.prompt.md` | Service interface + implementation + DTOs |
| `ct-ai-rules-handle-usecase.prompt.md` | Add [RelayCommand] UseCase method to existing ViewModel |
| `ct-ai-rules-cell.prompt.md` | DataGrid column template or ListBox ItemTemplate |
| `ct-ai-rules-unittest.prompt.md` | xUnit + Moq tests for ViewModel / UseCase / Repository |
| `generate-tests.prompt.md` | Quick test generation for any C# class |

### Design System

| File | Purpose |
|---|---|
| `ct-ai-rule-theme.prompt.md` | AppLabel / AppButton / AppTextField style keys and color tokens |

### General Patterns

| File | Purpose |
|---|---|
| `ct-ai-ask-for-input.prompt.md` | Generic ask-for-input pattern |
| `ct-ai-cognitive-verifier-pattern.prompt.md` | Verify assumptions before coding |
| `ct-ai-fact-checklist-pattern.prompt.md` | Fact-check requirements |
| `ct-ai-few-show-example-pattern.prompt.md` | Few-shot examples for code generation |
| `ct-ai-question-refinement-pattern.prompt.md` | Refine vague questions |
| `ct-ai-semantic-filter-pattern.prompt.md` | Filter and clean requirements |
| `ct-ai-all-files-impact-pr.prompt.md` | Identify all files impacted by a change |

---

## Usage

In VS Code Copilot Chat, type `/` to reference a prompt file, or open it and use `Run Prompt`.
