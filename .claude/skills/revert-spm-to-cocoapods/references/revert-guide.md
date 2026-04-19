# Reverting SPM Back to CocoaPods — Checklist & Pitfalls

## Why You Would Revert

The most common reason: SPM only builds the **native architecture** of the build machine.
On Apple Silicon, `xcodebuild` with `ARCHS=x86_64` (used by CI/CD) cannot find symbols in
SPM-built binaries → linker error: `Undefined symbol: <Package>.<SomeSymbol>`.

CocoaPods builds **fat frameworks** with both `arm64` and `x86_64` slices, so CI works on both.

---

## The Dual-Linking Crash (SIGABRT)

### Symptom

```
Object of class <SomeClass> deallocated with non-zero retain count N
*** SIGABRT in Container.persistedInstance<SomeClass>
```

### Root Cause

When a package is declared in **both** CocoaPods (via `Podfile`) **and** SPM (still referenced in
`ChoTot.xcodeproj`), the linker bundles **two separate copies** of the library's types into the
binary. The runtime sees two distinct `SomeClass` objects — reference counting gets confused and
the app crashes.

### Example (Swinject)

`ServiceEntry.storage` uses `[unowned self]` capturing `ObjectScope`. With two `ObjectScope`
classes in the binary, the captured instance can be deallocated while the lazy var hasn't yet
run — causing the retain-count crash.

---

## ⚠️ Critical: Sub-Module References Must Also Be Cleaned

> **When you revert a package from SPM to CocoaPods, you MUST remove SPM references from
> every sub-project `.xcodeproj`, not just the main `ChoTot.xcodeproj`.**

Feature and library sub-projects (e.g. `CTCommon`, `CTInsertAd`, `CTPrivateDashboard`) each
maintain their own SPM package references. If any sub-project still links the package via SPM
while the main target links it via CocoaPods, the result is a **version-mismatch linker error**.

### Example (SwiftDate)

`Package.resolved` pinned SPM SwiftDate **6.3.1** while Podfile used CocoaPods SwiftDate **7.0.0**.
In 7.x `defaultRegion` became a member of a `SwiftDate` type; in 6.x it was a top-level global.
Sub-project objects compiled against 6.x, main target linked 7.x → linker error:

```
Undefined symbol: SwiftDate.SwiftDate.defaultRegion.unsafeMutableAddressor : SwiftDate.Region
Linker command failed with exit code 1
```

---

## Revert Checklist

### 1. Remove SPM from main `ChoTot.xcodeproj/project.pbxproj`

For each reverted package, delete all of these entry types:

- `PBXBuildFile` — `/* <Product> in Frameworks */`
- Entry in `PBXFrameworksBuildPhase` files list
- Entry in ChoTot target `packageProductDependencies`
- Entry in project `packageReferences`
- `XCRemoteSwiftPackageReference` object block
- `XCSwiftPackageProductDependency` object block

### 2. Find all sub-projects that reference the package

```bash
grep -rl "<package-url>" AppFeatures/ Libraries/ --include="project.pbxproj"
```

### 3. Remove SPM from every sub-project

Use the xcodeproj gem (Ruby) to remove list references:

```ruby
# bin/remove_<package>_spm.rb
require 'xcodeproj'

PACKAGE_URL = 'https://github.com/owner/repo'
PRODUCT_NAME = 'ProductName'

PROJECTS.each do |rel_path|
  project = Xcodeproj::Project.open(File.join(Dir.pwd, rel_path))

  # Remove XCRemoteSwiftPackageReference
  project.root_object.package_references.delete_if do |ref|
    ref.respond_to?(:repositoryURL) && ref.repositoryURL == PACKAGE_URL
  end

  # Remove per-target deps + build phase entries
  project.targets.each do |target|
    target.package_product_dependencies.delete_if { |d| d.product_name == PRODUCT_NAME }
    next unless target.frameworks_build_phase
    target.frameworks_build_phase.files.delete_if do |f|
      f.product_ref.respond_to?(:product_name) && f.product_ref.product_name == PRODUCT_NAME
    rescue StandardError
      false
    end
  end

  project.save
end
```

### 4. Remove orphaned object blocks (xcodeproj gem limitation)

The gem removes list entries but leaves orphaned object block definitions in the `.pbxproj`.
Clean them with a Python regex pass:

```python
import re, os

pkg_ref_pattern = re.compile(
    r'\t\t[A-F0-9]+ /\* XCRemoteSwiftPackageReference "<repo-name>" \*/ = \{[^\}]+\{[^\}]+\}[^\}]+\};\n',
    re.MULTILINE)
prod_dep_pattern = re.compile(
    r'\t\t[A-F0-9]+ /\* <ProductName> \*/ = \{\n\t\t\tisa = XCSwiftPackageProductDependency;\n(?:\t\t\t[^\n]+\n)*\t\t\};\n',
    re.MULTILINE)
build_file_pattern = re.compile(
    r'\t\t[A-F0-9]+ /\* <ProductName> in Frameworks \*/ = \{[^\}]+\};\n',
    re.MULTILINE)

for path in project_paths:
    content = open(path).read()
    for pat in [pkg_ref_pattern, prod_dep_pattern, build_file_pattern]:
        content = pat.sub('', content)
    open(path, 'w').write(content)
```

### 5. Verify no references remain

```bash
grep -rl "<package-url>" AppFeatures/ Libraries/ --include="project.pbxproj"
# Should return nothing
```

### 6. Remove stale pins from `Package.resolved`

```python
import json
path = "ChoTot.xcworkspace/xcshareddata/swiftpm/Package.resolved"
data = json.load(open(path))
data["pins"] = [p for p in data["pins"] if p["identity"] not in {"<identity>"}]
json.dump(data, open(path, "w"), indent=2)
```

### 7. Run `pod install` + Clean Build Folder

```bash
pod install
# Then in Xcode: Shift+Cmd+K, then build
```

---

## Packages Reverted in This Project

| Package | Reason | Sub-projects affected |
|---|---|---|
| `SwiftDate` | `ARCHS=x86_64` CI failure (SPM arm64-only) | 19 sub-projects |
| `Lottie` | Same — arm64/x86_64 CI issue | 9 sub-projects |
| `Swinject` + `SwinjectAutoregistration` | Same + dual-linking SIGABRT crash | 16 sub-projects |

The `bin/sync_spm_packages.rb` post_integrate hook **excludes** these packages from
re-injection — they will not be re-added on subsequent `pod install` runs.

---

## Helper Scripts

Reusable removal scripts live in `bin/`:

- `bin/remove_swiftdate_spm.rb`
- `bin/remove_lottie_spm.rb`
- `bin/remove_swinject_spm.rb`

These are single-use cleanup scripts (run once per revert). They can be deleted after use.
