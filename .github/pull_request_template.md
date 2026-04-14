# Pull Request Template

## Description

Please include a summary of the change and which issue is fixed. Please also include relevant motivation and context. List any dependencies that are required for this change.

Fixes # (issue)

## Type of change

Please delete options that are not relevant.

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] This change requires a documentation update

## How Has This Been Tested?

Please describe the tests that you ran to verify your changes. Provide instructions so we can reproduce. Please also list any relevant details for your test configuration

## Checklist:

- Are there any merge conflicts or out of date in the PR?
- Have you passed all swiftlint check? Make sure No compilation error or warning?
- The architecture
    - Using MVVM + Clean, dependency injector that chapter has defined
    - Using CTTracking for tagging
    - No import feature module to feature module
- Is the code consistent with the agreed coding guidelines?
- Make sure UI using CTDesignSystem
- Memory leaks
    - Closures should use weak self
    - Delegates should be weak
    - Check if unowned is misused
    - Check for any retain cycles
- Remove unnecessary code
    - Remove commented code
    - Remove empty and/or unused variables, functions, imports…
    - Remove all hard code value
- Check naming
    - Is naming clear and consistent. The naming is important for writing self-documenting code?
    - Boolean values should start with is, can, should, will…
    - When in doubt - use longer names (adds more information) over shorter names (can bring confusion)
- Localization, do you have any unlocalized strings. All strings shall be localized.
- New framework announced with chapter lead
- Have Unittest for new features?
- [ ] Have you self-checked to ensure your code fully follows all the rules?




