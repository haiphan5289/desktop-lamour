
# [AI] Jira Ticket List Template

## 🎯 **Template for Listing Jira Tickets in Current Sprint**

### **Input Information Required:**
1. **Sprint Name**: `{sprintName}` (e.g. Revenue 25.30)
2. **Statuses**: `{statuses}` (default: 'New', 'In Progress')
3. **Container Context**: `{containerContext}` (e.g. Deeplink)

> 💡 **Assignee**: Automatically uses current user (`currentUser`) - no need to specify manually

---

### **Implementation Steps:**

#### **Step 1: Get Tickets in Current Sprint (FORCE SPRINT FILTER)**
**CRITICAL:** Use `mcp_atlassian_mcp_get_sprint_info` with `type="current"` and `includeAssignments=true` to get tickets that are **ONLY** in the current active sprint. **DO NOT** use `get_sprint_assignments` which fetches ALL user tickets. Must filter by:
- Current Sprint: Use `type="current"` to enforce sprint boundary
- Assignee: `"currentUser"` (automatically uses current user)
- Status: `{statuses}` (filter from sprint results)
- Force sprint boundary: Only tickets actually IN the current sprint, not all user tickets

#### **Step 2: Mark Special Tickets (Single Pass)**
From the SINGLE sprint result, identify and mark tickets by checking title/summary for keywords:
- **🟦 [deeplink]**: Keywords "deeplink", "Deeplink", "POS", "parser", "handler"
- **🟡 [API]**: Keywords "API"
**NO additional API calls needed**.

#### **Step 3: Deeplink Work Command**
After the list is generated, when the command `work deeplink [ticket-id]` is entered, automatically:
1. **LOG FILES READING**: Display which prompt files are being read:
   ```
   📄 Reading files for deeplink implementation:
   - prompt_get_description_cppf.md
   - deeplink_common_prompt.md
   ```
2. Run the prompt from `prompt_get_description_cppf.md` to get CPPF parent description and Jira link
3. **MUST APPLY CODE** to all relevant files (parser, handler, etc.) following the 5-step process from `deeplink_common_prompt.md`
4. **DO NOT** just generate code blocks - must use `replace_string_in_file` tool to actually apply changes to files

#### **Step 4: API UseCase Work Command**
After the list is generated, when the command `work api usecase [ticket-id]` is entered, automatically:
1. **LOG FILES READING**: Display which prompt files are being read:
   ```
   📄 Reading files for API UseCase implementation:
   - usecasse_get_description_cppf.md
   - AI_generate_usecase_template.md
   ```
2. Run the prompt from `usecasse_get_description_cppf.md` to get CPPF parent description and auto-generate UseCase
3. **MUST APPLY CODE** following the 6-layer architecture pattern from `AI_generate_usecase_template.md`
4. **DO NOT** just generate code blocks - must use `replace_string_in_file` tool to actually apply changes to files

---


### **Example Usage:**


```markdown
[AI] Jira Ticket List:
1. Sprint Name: Revenue 25.30 (Current Sprint)
2. Statuses: New, In Progress
3. Container Context: Deeplink (for marking only)
```

> 💡 *Just enter the prompt above and the system will automatically execute the full workflow as follows:*
> 1. **SINGLE API CALL**: Use `mcp_atlassian_mcp_get_sprint_info` with `type="current"` to get tickets ONLY in the current sprint
> 2. **FORCE SPRINT BOUNDARY**: Only process tickets that are actually IN the current sprint, not all user tickets
> 3. **EFFICIENT PROCESSING**: Filter by assignee and status from sprint results, mark 🟦 [deeplink] and 🟡 [API] in one pass
> 4. When you enter `work deeplink [ticket-id]`, the system will automatically:
>    - **LOG FILES**: Display which files are being read for deeplink implementation
>    - Fetch CPPF parent description
>    - Generate instruction block following `deeplink_common_prompt.md` format
>    - **APPLY CODE CHANGES** to all relevant files (RevenueDeeplinkParser.swift, RevenueDeeplinkHandler.swift, etc.)
>    - **NEVER** just show code blocks - must use file editing tools to make actual changes
> 5. When you enter `work api usecase [ticket-id]`, the system will automatically:
>    - **LOG FILES**: Display which files are being read for API UseCase implementation
>    - Fetch CPPF parent description from `usecasse_get_description_cppf.md`
>    - Auto-generate UseCase following 6-layer architecture pattern
>    - **APPLY CODE CHANGES** to all relevant UseCase files (Targets, Services, Repositories, UseCase, ViewModel)
>    - **NEVER** just show code blocks - must use file editing tools to make actual changes
> 6. **CRITICAL:** All 5 steps from `deeplink_common_prompt.md` must be completed with actual file modifications, not just code generation.

> 💡 *Enter this prompt to automatically fetch the ticket list from Jira for the specified assignee, sprint, statuses, and context.*

---


### **🔍 Implementation Checklist:**
- [ ] **SINGLE API CALL**: Use `mcp_atlassian_mcp_get_sprint_info` with `type="current"` and `includeAssignments=true`
- [ ] **FORCE SPRINT FILTER**: Only tickets IN the current sprint, not all user tickets
- [ ] **AUTO ASSIGNEE**: Use `assignee="currentUser"` to automatically get current user's tickets
- [ ] **POST-FILTER BY STATUS**: Filter results by `{statuses}` from sprint assignments  
- [ ] **EFFICIENT PROCESSING**: Mark deeplink and API tickets from filtered result set
- [ ] On `work deeplink [ticket-id]` command:
  - [ ] **LOG FILES**: Display which prompt files are being read for implementation
  - [ ] Run `prompt_get_description_cppf.md` for the specific ticket
  - [ ] **APPLY CODE** using `replace_string_in_file` tool (NOT just generate code blocks)
  - [ ] Complete all 5 steps from `deeplink_common_prompt.md` with actual file modifications
- [ ] On `work api usecase [ticket-id]` command:
  - [ ] **LOG FILES**: Display which prompt files are being read for implementation
  - [ ] Run `usecasse_get_description_cppf.md` for the specific ticket
  - [ ] **APPLY CODE** using `replace_string_in_file` tool (NOT just generate code blocks)
  - [ ] Complete all 6-layer architecture implementation with actual file modifications

---

### **⚠️ Important Notes:**
1. **SPRINT ENFORCEMENT**: ALWAYS use `type="current"` with `mcp_atlassian_mcp_get_sprint_info` - no fetching all tickets
2. **SINGLE API CALL**: Use `mcp_atlassian_mcp_get_sprint_info` only once, then filter results by status
3. **AUTO ASSIGNEE**: Always use `assignee="currentUser"` to automatically get current user's tickets
4. **EFFICIENCY**: No `get_sprint_assignments` calls - it fetches ALL user tickets across sprints
5. **Correct Filtering**: Get sprint assignments first, then filter by status in code (assignee is auto-handled)
6. **Ticket Detection**: 
   - **Deeplink tickets**: Use title/summary keywords: "deeplink", "Deeplink", "POS", "parser", "handler"
   - **API tickets**: Use title/summary keywords: "API"
7. **Output Format**: Use colored labels 🟦 [deeplink] and 🟡 [API] for clarity
8. **Automation**: The `work deeplink [ticket-id]` command should trigger the next prompt automatically with file logging
9. **API UseCase Automation**: The `work api usecase [ticket-id]` command should trigger `usecasse_get_description_cppf.md` automatically with file logging
10. **FILE LOGGING**: Always display which prompt files are being read before execution
11. **CRITICAL**: **MUST APPLY CODE TO FILES** - Never just show code blocks, always use file editing tools
12. **File Changes**: All deeplink implementations must modify actual Swift files using `replace_string_in_file`
13. **UseCase Changes**: All API UseCase implementations must modify actual Swift files using `replace_string_in_file`