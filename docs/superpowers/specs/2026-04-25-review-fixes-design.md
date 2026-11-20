# Review Fixes Design

## Goal

Apply the approved PR review fixes with the smallest possible code change set, preserve the current public API where practical, and add regression tests for each reviewed behavior before pushing the branch.

## Scope

This change set covers:

- Fixing `ZaSpanStringBuilderExtensions.RemoveLast()` after the `Advance()` validation change.
- Making query parameter helpers failure-atomic, including the `ref bool isFirst` overload.
- Completing interpolated alignment support for common `string` and `ReadOnlySpan<char>` inputs.
- Removing the arbitrary retry cap from `ZaPooledStringBuilder.Append<T>()`.
- Hardening reviewed edge cases in `TryAppendLine()`, pooled capacity growth, and URL/form encoding for lone surrogates.
- Adding direct regression tests for all of the above.

This change set does not include unrelated refactoring or API redesign.

## Design

### Mutation helpers

`ZaSpanStringBuilderExtensions.RemoveLast()` will stop calling `Advance(-count)` and instead truncate through `SetLength(builder.Length - count)`. This matches the current builder contract and keeps the extension behavior aligned with the struct instance method.

### Query parameter atomicity

`AppendQueryParam()` will be changed to compute the total required output length first, using the same encoding rules as the eventual write path. Only after the full operation is proven to fit will the method append delimiters and encoded content. The `ref bool isFirst` overload will update `isFirst` only after a successful append.

### Interpolated alignment

`ZaInterpolatedStringHandler` will gain aligned overloads for `string?` and `ReadOnlySpan<char>`, with behavior matching standard composite-format alignment semantics. These overloads will preserve atomic failure semantics: if the aligned write cannot fit, they will throw without partially advancing the builder.

### Pooled formatting growth

`ZaPooledStringBuilder.Append<T>()` will no longer fail after a fixed retry count. Instead, it will continue growing until formatting succeeds or capacity growth reaches a real limit enforced by `EnsureCapacity()`.

### Edge-case hardening

- `TryAppendLine(string?)` will use overflow-safe capacity checks.
- `ZaPooledStringBuilder.EnsureCapacity()` will clamp growth to `Array.MaxLength` while still honoring the required size.
- URL/form encoding helpers will treat lone surrogates consistently with UTF-8 replacement-character behavior instead of emitting invalid UTF-8 percent-encodings.

## Testing

Add focused regression tests for:

- `RemoveLast()` truncation on the extension path.
- Failure-atomic `AppendQueryParam()` behavior and `isFirst` state preservation on failure.
- Aligned string and span interpolation.
- Large or repeatedly-growing `ISpanFormattable` formatting behavior without an artificial retry cap.
- `TryAppendLine()` overflow-safe atomic failure.
- Lone-surrogate URL/form encoding output.

## Verification

Run the relevant `ZaString.Tests` project tests on the available framework in this environment and review the resulting diff before pushing. If multi-target execution is blocked by missing runtimes, record that explicitly and verify on the installed target framework.
