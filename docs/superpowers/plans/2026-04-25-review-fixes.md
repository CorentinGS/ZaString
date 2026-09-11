# Review Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the approved PR review issues, add regression coverage, and push the branch safely.

**Architecture:** Keep the current API surface and file layout intact. Apply targeted fixes in the affected builder, interpolation, and encoding helpers, then add focused tests that prove the reviewed failure modes are resolved.

**Tech Stack:** C#, .NET 10 test execution in this environment, xUnit, Jujutsu (`jj`)

---

### Task 1: Fix Span Builder Mutation And Query Atomicity

**Files:**
- Modify: `src/ZaString/Extensions/ZaSpanStringBuilderExtensions.cs`
- Test: `tests/ZaString.Tests/ZaSpanStringBuilderMutationHelpersTests.cs`
- Test: `tests/ZaString.Tests/ZaSpanStringBuilderUrlHelpersTests.cs`

- [ ] **Step 1: Write the failing tests**

Add these tests.

In `tests/ZaString.Tests/ZaSpanStringBuilderMutationHelpersTests.cs`:

```csharp
[Fact]
public void RemoveLast_Extension_RemovesCorrectly()
{
    Span<char> buffer = stackalloc char[16];
    var builder = ZaSpanStringBuilder.Create(buffer);
    builder.Append("abcdef");

    ZaSpanStringBuilderExtensions.RemoveLast(ref builder, 2);

    Assert.Equal("abcd", builder.AsSpan());
    Assert.Equal(4, builder.Length);
}
```

In `tests/ZaString.Tests/ZaSpanStringBuilderUrlHelpersTests.cs`:

```csharp
[Fact]
public void AppendQueryParam_WithRefBool_Failure_IsAtomic()
{
    Span<char> buffer = stackalloc char[8];
    var builder = ZaSpanStringBuilder.Create(buffer);
    builder.Append("/s");

    var isFirst = true;

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        builder.AppendQueryParam("longkey", "x", ref isFirst));

    Assert.Equal("/s", builder.AsSpan());
    Assert.True(isFirst);
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~RemoveLast_Extension_RemovesCorrectly|FullyQualifiedName~AppendQueryParam_WithRefBool_Failure_IsAtomic"`

Expected: FAIL with `RemoveLast` throwing and/or the query helper leaving partial state.

- [ ] **Step 3: Write the minimal implementation**

Update `src/ZaString/Extensions/ZaSpanStringBuilderExtensions.cs`.

Replace the `RemoveLast` body with truncation via `SetLength`:

```csharp
public static ref ZaSpanStringBuilder RemoveLast(ref this ZaSpanStringBuilder builder, int count)
{
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    if (count == 0)
    {
        return ref builder;
    }

    if (builder.Length < count)
    {
        ThrowOutOfRangeException();
    }

    builder.SetLength(builder.Length - count);
    return ref builder;
}
```

Make `AppendQueryParam` atomic by precomputing required length and mutating `isFirst` only after success. Add shared helpers near the query helper section:

```csharp
private static int GetQueryParamLength(ReadOnlySpan<char> key, ReadOnlySpan<char> value, bool urlEncode)
{
    var keyLength = urlEncode ? GetUrlEncodedLengthReplacingInvalid(value: key) : key.Length;
    var valueLength = urlEncode ? GetUrlEncodedLengthReplacingInvalid(value) : value.Length;
    return 1 + keyLength + 1 + valueLength;
}

private static void WriteQueryParam(Span<char> destination, char prefix, ReadOnlySpan<char> key, ReadOnlySpan<char> value, bool urlEncode)
{
    destination[0] = prefix;
    var written = 1;

    if (urlEncode)
    {
        written += WriteUrlEncodedReplacingInvalid(key, destination[written..]);
        destination[written++] = '=';
        written += WriteUrlEncodedReplacingInvalid(value, destination[written..]);
        return;
    }

    key.CopyTo(destination[written..]);
    written += key.Length;
    destination[written++] = '=';
    value.CopyTo(destination[written..]);
}
```

Then rewrite both overloads to use those helpers:

```csharp
public static ref ZaSpanStringBuilder AppendQueryParam(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> key, ReadOnlySpan<char> value, bool urlEncode = true, bool isFirst = false)
{
    var required = GetQueryParamLength(key, value, urlEncode);
    if (required > builder.RemainingSpan.Length)
    {
        ThrowOutOfRangeException();
    }

    WriteQueryParam(builder.RemainingSpan, isFirst ? '?' : '&', key, value, urlEncode);
    builder.Advance(required);
    return ref builder;
}

public static ref ZaSpanStringBuilder AppendQueryParam(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> key, ReadOnlySpan<char> value, ref bool isFirst, bool urlEncode = true)
{
    var required = GetQueryParamLength(key, value, urlEncode);
    if (required > builder.RemainingSpan.Length)
    {
        ThrowOutOfRangeException();
    }

    WriteQueryParam(builder.RemainingSpan, isFirst ? '?' : '&', key, value, urlEncode);
    builder.Advance(required);
    isFirst = false;
    return ref builder;
}
```

- [ ] **Step 4: Run the targeted tests to verify they pass**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~RemoveLast_Extension_RemovesCorrectly|FullyQualifiedName~AppendQueryParam_WithRefBool_Failure_IsAtomic"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
jj desc -m "Fix builder mutation and query atomicity"
jj new -m "Add interpolation and encoding review fixes"
jj st
```

### Task 2: Complete Alignment Support And Remove Artificial Retry Limit

**Files:**
- Modify: `src/ZaString/Extensions/ZaInterpolatedStringHandler.cs`
- Modify: `src/ZaString/Core/ZaPooledStringBuilder.cs`
- Test: `tests/ZaString.Tests/ZaSpanStringBuilderInterpolationTests.cs`
- Test: `tests/ZaString.Tests/ZaPooledStringBuilderTests.cs`

- [ ] **Step 1: Write the failing tests**

In `tests/ZaString.Tests/ZaSpanStringBuilderInterpolationTests.cs`, add:

```csharp
[Fact]
public void Append_InterpolatedString_WithAlignedString_Works()
{
    Span<char> buffer = stackalloc char[32];
    var builder = ZaSpanStringBuilder.Create(buffer);

    var name = "Ada";
    builder.Append($"|{name,6}|{name,-6}|");

    Assert.Equal("|   Ada|Ada   |", builder.AsSpan());
}
```

In `tests/ZaString.Tests/ZaPooledStringBuilderTests.cs`, add a formattable that succeeds only once capacity reaches a large threshold and verify append succeeds:

```csharp
public readonly struct LargeFormattable : ISpanFormattable
{
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < 2_000_000)
        {
            charsWritten = 0;
            return false;
        }

        "big".AsSpan().CopyTo(destination);
        charsWritten = 3;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => "big";
}

[Fact]
public void Append_LargeISpanFormattable_GrowsUntilItSucceeds()
{
    using var builder = ZaPooledStringBuilder.Rent(4);

    builder.Append(new LargeFormattable());

    Assert.Equal("big", builder.ToString());
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~Append_InterpolatedString_WithAlignedString_Works|FullyQualifiedName~Append_LargeISpanFormattable_GrowsUntilItSucceeds"`

Expected: FAIL because aligned string interpolation is unsupported and the pooled append hits the retry limit.

- [ ] **Step 3: Write the minimal implementation**

In `src/ZaString/Extensions/ZaInterpolatedStringHandler.cs`, add aligned overloads that share a small helper:

```csharp
public void AppendFormatted(string? value, int alignment)
{
    AppendAligned(value is null ? ReadOnlySpan<char>.Empty : value.AsSpan(), alignment);
}

public void AppendFormatted(string? value, int alignment, string? format)
{
    AppendAligned(value is null ? ReadOnlySpan<char>.Empty : value.AsSpan(), alignment);
}

public void AppendFormatted(ReadOnlySpan<char> value, int alignment)
{
    AppendAligned(value, alignment);
}

public void AppendFormatted(ReadOnlySpan<char> value, int alignment, string? format)
{
    AppendAligned(value, alignment);
}

private void AppendAligned(ReadOnlySpan<char> value, int alignment)
{
    var width = Math.Abs(alignment);
    var totalWidth = Math.Max(width, value.Length);
    var remaining = _builder.RemainingSpan;

    if (remaining.Length < totalWidth)
    {
        throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
    }

    var padCount = totalWidth - value.Length;
    if (alignment > 0)
    {
        remaining[..padCount].Fill(' ');
        value.CopyTo(remaining[padCount..]);
    }
    else
    {
        value.CopyTo(remaining);
        remaining.Slice(value.Length, padCount).Fill(' ');
    }

    _builder.Advance(totalWidth);
}
```

In `src/ZaString/Core/ZaPooledStringBuilder.cs`, remove the retry counter and keep the growth loop bounded only by `EnsureCapacity()`:

```csharp
public ZaPooledStringBuilder Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
{
    ThrowIfDisposed();
    provider ??= CultureInfo.InvariantCulture;

    while (true)
    {
        if (value.TryFormat(_buffer.AsSpan(Length), out var written, format, provider))
        {
            Length += written;
            return this;
        }

        var remaining = _buffer.Length - Length;
        var growBy = remaining + 1;
        EnsureCapacity(growBy);
    }
}
```

- [ ] **Step 4: Run the targeted tests to verify they pass**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~Append_InterpolatedString_WithAlignedString_Works|FullyQualifiedName~Append_LargeISpanFormattable_GrowsUntilItSucceeds"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
jj desc -m "Add interpolation and encoding review fixes"
jj new -m "Harden review edge cases and tests"
jj st
```

### Task 3: Harden Edge Cases, Verify, And Push

**Files:**
- Modify: `src/ZaString/Extensions/ZaSpanStringBuilderExtensions.cs`
- Modify: `src/ZaString/Core/ZaPooledStringBuilder.cs`
- Test: `tests/ZaString.Tests/ZaSpanStringBuilderTryAppendTests.cs`
- Test: `tests/ZaString.Tests/ZaSpanStringBuilderUrlHelpersTests.cs`
- Test: `tests/ZaString.Tests/ZaPooledStringBuilderTests.cs`

- [ ] **Step 1: Write the failing tests**

Add these tests.

In `tests/ZaString.Tests/ZaSpanStringBuilderUrlHelpersTests.cs`:

```csharp
[Fact]
public void AppendFormUrlEncoded_LoneHighSurrogate_UsesReplacementCharacter()
{
    Span<char> buffer = stackalloc char[32];
    var builder = ZaSpanStringBuilder.Create(buffer);

    builder.AppendFormUrlEncoded("\uD800");

    Assert.Equal("%EF%BF%BD", builder.AsSpan());
}
```

In `tests/ZaString.Tests/ZaSpanStringBuilderTryAppendTests.cs`, add coverage for a subtraction-based helper that replaces the overflow-prone `required = valueLength + newlineLength` arithmetic:

```csharp
[Theory]
[InlineData(5, 2, 7, true)]
[InlineData(5, 2, 6, false)]
[InlineData(int.MaxValue, 2, int.MaxValue, false)]
public void HasCapacityForLine_UsesOverflowSafeArithmetic(int valueLength, int newlineLength, int remainingLength, bool expected)
{
    var hasCapacityForLine = typeof(ZaSpanStringBuilderExtensions)
        .GetMethod("HasCapacityForLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

    Assert.NotNull(hasCapacityForLine);
    var actual = (bool)hasCapacityForLine.Invoke(null, new object[] { valueLength, newlineLength, remainingLength })!;
    Assert.Equal(expected, actual);
}
```

In `tests/ZaString.Tests/ZaPooledStringBuilderTests.cs`, add a clamp-oriented reflection test for the growth helper introduced in this task:

```csharp
[Theory]
[InlineData(200, 300, 300)]
[InlineData(200, 100, 300)]
[InlineData(0, 1, 256)]
public void ComputeExpandedCapacity_ClampsAndGrowsSafely(int currentCapacity, int required, int expected)
{
    var computeExpandedCapacity = typeof(ZaPooledStringBuilder)
        .GetMethod("ComputeExpandedCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

    Assert.NotNull(computeExpandedCapacity);
    var actual = (int)computeExpandedCapacity.Invoke(null, new object[] { currentCapacity, required })!;
    Assert.Equal(expected, actual);
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~AppendFormUrlEncoded_LoneHighSurrogate_UsesReplacementCharacter"`

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0 --filter "FullyQualifiedName~HasCapacityForLine_UsesOverflowSafeArithmetic|FullyQualifiedName~ComputeExpandedCapacity_ClampsAndGrowsSafely"`

Expected: FAIL with the current invalid surrogate encoding behavior and missing helper methods.

- [ ] **Step 3: Write the minimal implementation**

In `src/ZaString/Extensions/ZaSpanStringBuilderExtensions.cs`:

- Add an internal helper that avoids addition overflow and call it from `TryAppendLine(string?)`.
- Refactor URL/form encoding length and write paths to treat lone surrogates as U+FFFD and reuse the same helper for query parameter size computation.

Use helpers like:

```csharp
private static bool HasCapacityForLine(int valueLength, int newlineLength, int remainingLength)
{
    return valueLength <= remainingLength && newlineLength <= remainingLength - valueLength;
}

private static int GetUrlEncodedLengthReplacingInvalid(ReadOnlySpan<char> value)
private static int WriteUrlEncodedReplacingInvalid(ReadOnlySpan<char> value, Span<char> destination)
```

and in the lone-surrogate branch percent-encode UTF-8 bytes for `0xFFFD`.

In `src/ZaString/Core/ZaPooledStringBuilder.cs`, clamp growth:

```csharp
private static int ComputeExpandedCapacity(int currentCapacity, int required)
{
    var grown = currentCapacity <= Array.MaxLength - (currentCapacity / 2)
        ? currentCapacity + (currentCapacity / 2)
        : Array.MaxLength;

    var newCapacity = Math.Max(required, grown);
    if (newCapacity < 256)
    {
        newCapacity = 256;
    }

    return Math.Min(newCapacity, Array.MaxLength);
}
```

and update `EnsureCapacity` to call `ComputeExpandedCapacity(_buffer.Length, required)` before renting.

- [ ] **Step 4: Run verification**

Run: `dotnet test tests/ZaString.Tests/ZaString.Tests.csproj -f net10.0`

Expected: PASS all tests on `net10.0`.

Run: `jj diff`

Expected: Only the approved review-fix files and tests are changed.

- [ ] **Step 5: Push**

```bash
jj desc -m "Harden review edge cases and tests"
jj bookmark list
jj bookmark move feature/zastring-final --to @
jj git push -b feature/zastring-final
jj st
```

Expected: bookmark moves to the current change and the branch pushes successfully.
