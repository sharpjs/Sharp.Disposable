# Sharp.Disposable

Thread-safe implementation of the [.NET Dispose Pattern](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose), plus extras.

## Status

[![Build](https://github.com/sharpjs/Sharp.Disposable/workflows/Build/badge.svg)](https://github.com/sharpjs/Sharp.Disposable/actions)
[![NuGet](https://img.shields.io/nuget/v/Sharp.Disposable.svg)](https://www.nuget.org/packages/Sharp.Disposable)
[![NuGet](https://img.shields.io/nuget/dt/Sharp.Disposable.svg)](https://www.nuget.org/packages/Sharp.Disposable)

- **Stable:**     in production for years with no reported defects.
- **Tested:**     100% coverage by automated tests.
- **Documented:** IntelliSense on everything.

## Overview

This package provides the following types in the `Sharp.Disposable` namespace:

Name             | Description
-----------------|------------
`Disposable`     | Base class for any disposable object.<br>– Thread-safe; dispose from any thread.<br>– Guarantees that disposal happens only once.<br>– Provides an `IsDisposed` property.<br>– Provides a `RequireNotDisposed()` helper method.
`DisposableBox`  | Generic, mutable box that can hold a single disposable object.<br>– The object can be *owned* or *borrowed*.<br>– Disposes an owned object when another is placed in the box.<br>– Disposes an owned object when the box itself is disposed.
`DisposablePool` | Collects multiple disposable objects, disposing them when the pool itself is disposed.
`Finalizer`      | Methods to force object finalization.

## Usage

```csharp
using Sharp.Disposable;

public class Foo : Disposable
{
    private SomeDisposable _bar; // a managed resource

    // ...
    
    protected override bool Dispose(bool managed)
    {
        // Check if already disposed
        if (!base.Dispose(managed))
            // False means nothing happened because already disposed
            return false;

        // Clean up unmanaged resources (like temp files) here
        DeleteTemporaryFiles();

        // Check if doing managed disposal too
        if (managed)
            // Disposed managed resources (other IDisposables) here
            _bar.Dispose();

        // True means disposal happened
        return true;
    }
}
```

<!--
  Copyright 2022 Jeffrey Sharp

  Permission to use, copy, modify, and distribute this software for any
  purpose with or without fee is hereby granted, provided that the above
  copyright notice and this permission notice appear in all copies.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
  WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
  MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
  ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
  WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
  ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
  OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
-->
