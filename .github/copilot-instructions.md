## Basic style rules

You MUST follow all code-formatting and naming conventions defined in [`.editorconfig`](/.editorconfig).

You MUST NOT use any code from FluentAssertions because of licensing issues. This includes copying code from FluentAssertions, even if you modify it.

In addition to the rules enforced by .editorconfig, you SHOULD:
- `var` is not allowed unless type is obvious.
- `using` directives must be outside namespace.
- max line length for C# is 130.
- Use pattern matching and switch expressions wherever possible.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Do not use `type.Name` or `predicate.Body` directly for assertion messages; use `FailWith` formatting.
- Fluent API should read naturally; avoid awkward chain wording.
- test method names should describe scenario and outcome; avoid technical `When`/`Should` clutter.
- No region directives.

## Assertions implementation guidance
- Keep API extensible, preserve type info in generic assertions (use `AndWhichConstraint` where applicable).
- Handle null and `AssertionScope` semantics properly (e.g., `FailWith` may not throw immediately).
- Provide `because` message placeholder support in assertion APIs.

## Writing specs/tests (high priority)
1. Use folder naming in `Tests/AwesomeAssertions.Specs` (e.g. `Exceptions`, `Xml`, `Equivalency`).
2. Add tests in existing file or new file when appropriate, grouped in nested classes by API.
3. Use the AAA pattern with exactly one blank line between Arrange/Act/Assert.
4. For expected failures, assert `XunitException` and include message checks with `Match` or `WithMessage`.
5. Check `because` formatting using string placeholders and exact expected message behavior.
6. Prefer `TheoryData` for permutations and minimize duplicated code.
7. Do not use `Should().NotThrow` as a pass assertion pattern; it's reserved for explicit negative scenarios.

## Practical commands
- `dotnet test` (root or scoped `Tests/AwesomeAssertions.Specs`).

## Optional (less critical but encouraged)
- Prefer expression-bodied members in alignment with `.editorconfig` settings.
- Use C# keyword types (`int`, `string`) over BCL types (`System.Int32`).
- Apply code fixers/test updates for changed APIs and ensure coverage and message correctness.

## Notes
- This file is intentionally terse; `.editorconfig` and `DESIGN-GUIDE.md` contain full style guidance.
- Only non-obvious, repo-specific rules belong here.
