---
name: revert-spm-to-cocoapods
description: "Revert a previously-migrated SPM package back to CocoaPods in the ChoTot iOS monorepo. Handles dual-linking SIGABRT crashes and the critical sub-module cleanup (every AppFeatures/Libraries sub-project must be cleaned, not just ChoTot.xcodeproj). Use when CI fails with ARCHS=x86_64 undefined symbol errors or when a runtime crash shows 'deallocated with non-zero retain count' after SPM migration."
argument-hint: "[package1] [package2] ... — pod names as they appear in the Podfile (e.g. SwiftDate Lottie Swinject SwinjectAutoregistration)"
model: sonnet
effort: high
---

# SPM → CocoaPods Revert Skill

Revert one or more SPM packages back to CocoaPods in the **Chợ Tốt iOS** monorepo.

**Reference guide:** `references/revert-guide.md` — read before making any changes.

---

## When to Use

- CI linker error: `Undefined symbol: <Package>.<Symbol>` with `ARCHS=x86_64`
- Runtime SIGABRT: `Object of class X deallocated with non-zero retain count N`
- SPM package builds arm64-only slice; CI machine requires x86_64

---

## Examples

```bash
# Single package
/revert-spm-to-cocoapods SwiftDate

# Multiple packages — run sequentially, one pod install at the end
/revert-spm-to-cocoapods SwiftDate Lottie Swinject SwinjectAutoregistration

# With explicit pod name if different from SPM product name
/revert-spm-to-cocoapods Kingfisher
```

---

## ❗ Critical Rule

> **ALWAYS clean SPM references from EVERY sub-project `.xcodeproj` in `AppFeatures/` and
> `Libraries/`, not just `ChoTot.xcodeproj`.** Skipping this causes version-mismatch linker
> errors even after the main project is clean.

---

## Execution Flow

### Step 1 — Read the Reference Guide

```
references/revert-guide.md
```

### Step 2 — Confirm Inputs

Ask (or infer from context):
- Package name(s) to revert
- CocoaPods pod name (usually same as SPM product name)
- Whether the package is already declared in the `Podfile` as `pod 'X'`

### Step 3 — Find All Affected Projects

```bash
# For each package URL:
grep -rl "<github-url>" AppFeatures/ Libraries/ --include="project.pbxproj"
# Also check main project:
grep -c "<github-url>" ChoTot.xcodeproj/project.pbxproj
```

### Step 4 — Clean Main `ChoTot.xcodeproj`

Remove ALL of these entry types for the package:

| Entry type | Identifier pattern |
|---|---|
| `PBXBuildFile` | `/* <Product> in Frameworks */` |
| `PBXFrameworksBuildPhase` file ref | inside `files = (...)` of ChoTot target |
| `packageProductDependencies` entry | inside ChoTot target |
| `packageReferences` entry | at project root level |
| `XCRemoteSwiftPackageReference` object block | full `{ isa = ...; repositoryURL = ...; }` block |
| `XCSwiftPackageProductDependency` object block | full `{ isa = ...; package = ...; productName = ...; }` block |

### Step 5 — Clean All Sub-projects (via xcodeproj gem)

Create and run `bin/remove_<package>_spm.rb`:

```ruby
#!/usr/bin/env ruby
# frozen_string_literal: true
require 'xcodeproj'

PACKAGE_URL = 'https://github.com/owner/repo'
PRODUCT_NAMES = %w[ProductName].freeze  # add all products from this package

PROJECTS = %w[
  # paste the grep results from Step 3 here
].freeze

PROJECTS.each do |rel_path|
  project_path = File.join(Dir.pwd, rel_path)
  next puts "⚠️  Skip: #{rel_path}" unless File.exist?(project_path)

  project = Xcodeproj::Project.open(project_path)
  changed = false

  project.root_object.package_references.delete_if do |ref|
    next false unless ref.respond_to?(:repositoryURL)
    (ref.repositoryURL == PACKAGE_URL).tap { |r| changed = true if r }
  end

  project.targets.each do |target|
    target.package_product_dependencies.delete_if do |dep|
      PRODUCT_NAMES.include?(dep.product_name).tap { |r| changed = true if r }
    end

    next unless target.respond_to?(:frameworks_build_phase) && target.frameworks_build_phase
    target.frameworks_build_phase.files.delete_if do |file|
      (file.product_ref.respond_to?(:product_name) &&
        PRODUCT_NAMES.include?(file.product_ref.product_name)).tap { |r| changed = true if r }
    rescue StandardError
      false
    end
  end

  project.save
  puts changed ? "✅  Cleaned: #{rel_path}" : "⏭️  No refs: #{rel_path}"
end
```

Run: `ruby bin/remove_<package>_spm.rb`

### Step 6 — Remove Orphaned Object Blocks

The xcodeproj gem removes list references but leaves orphaned object block definitions.
Clean them with Python:

```python
import re, os

# Fill in: repo-name = the name after the last "/" in the GitHub URL
# Fill in: ProductName = the SPM product name

pkg_ref_pat = re.compile(
    r'\t\t[A-F0-9]+ /\* XCRemoteSwiftPackageReference "<repo-name>" \*/ = \{[^\}]+\{[^\}]+\}[^\}]+\};\n',
    re.MULTILINE)
prod_dep_pat = re.compile(
    r'\t\t[A-F0-9]+ /\* <ProductName> \*/ = \{\n\t\t\tisa = XCSwiftPackageProductDependency;\n(?:\t\t\t[^\n]+\n)*\t\t\};\n',
    re.MULTILINE)
build_file_pat = re.compile(
    r'\t\t[A-F0-9]+ /\* <ProductName> in Frameworks \*/ = \{[^\}]+\};\n',
    re.MULTILINE)

PROJECTS = [
    # same list as Step 5, but full paths to project.pbxproj
]

for rel in PROJECTS:
    path = os.path.join(os.getcwd(), rel, "project.pbxproj")
    if not os.path.exists(path): continue
    content = open(path).read()
    new = content
    for pat in [pkg_ref_pat, prod_dep_pat, build_file_pat]:
        new = pat.sub('', new)
    if new != content:
        open(path, 'w').write(new)
        print(f"✅ {rel}")
```

### Step 7 — Verify Clean

```bash
# Should return nothing for each reverted package
grep -rl "<github-url>" AppFeatures/ Libraries/ ChoTot.xcodeproj --include="project.pbxproj"
```

### Step 8 — Update Podfile (if not already using `pod 'X'`)

The Podfile helper for the package should call `pod 'X'` directly (not `[:spm, 'XPackage']`).

Example after revert:
```ruby
def spm_swift_date
  pod 'SwiftDate'   # was: [:spm, 'SwiftDatePackage']
end
```

Also ensure `bin/sync_spm_packages.rb` does **NOT** list this package in `SPM_PACKAGES` —
it should not be re-injected by the `post_integrate` hook.

### Step 9 — Remove Stale Pins from `Package.resolved`

```python
import json
path = "ChoTot.xcworkspace/xcshareddata/swiftpm/Package.resolved"
data = json.load(open(path))
REMOVE = {"<identity>"}   # lowercase identity from Package.resolved
data["pins"] = [p for p in data["pins"] if p["identity"] not in REMOVE]
with open(path, "w") as f:
    json.dump(data, f, indent=2)
    f.write("\n")
```

### Step 10 — `pod install` + Clean Build

```bash
pod install
# Then in Xcode: Shift+Cmd+K (Clean Build Folder), then Cmd+B
```

---

## Validation Checklist

- [ ] `grep` for package URL returns nothing in all `.xcodeproj` files
- [ ] `Package.resolved` no longer pins the reverted package(s)
- [ ] `pod install` completes without error
- [ ] Clean build succeeds (no undefined symbol linker errors)
- [ ] App launches without SIGABRT / retain count crash

---

## Known Reverted Packages in This Project

| Package | SPM Identity | Sub-projects cleaned | Helper script |
|---|---|---|---|
| SwiftDate | `swiftdate` | 19 | `bin/remove_swiftdate_spm.rb` |
| Lottie | `lottie-ios` | 9 | `bin/remove_lottie_spm.rb` |
| Swinject | `swinject` | 16 | `bin/remove_swinject_spm.rb` |
| SwinjectAutoregistration | `swinjectautoregistration` | 16 (same run) | `bin/remove_swinject_spm.rb` |

**Root cause for all:** SPM builds arm64-only simulator slice on Apple Silicon; CI uses
`ARCHS=x86_64` → undefined symbols. CocoaPods fat frameworks include both slices.

---

## 🧠 Claude Behavior Rules

- MUST read `references/revert-guide.md` before any file edits
- MUST search sub-projects before declaring the main project clean
- NEVER assume a package only appears in `ChoTot.xcodeproj`
- Run `ruby bin/remove_*.rb` directly (not via `bundle exec` — fastlane gems not installed)
- After running xcodeproj gem script, ALWAYS run the Python orphan-block cleanup
- Run `pod install` automatically — do not ask the user to run it
