# Sharp.Disposable

Thread-safe implementation of the [.NET Dispose Pattern](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern), plus extras.

## Status

**Private Beta Testing.**

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
            return false;

        // Clean up unmanaged resources (like temp files) here
        DeleteTemporaryFiles();

        // Check if doing unmanaged disposal only
        if (!managed)
            return true;

        // Disposed managed resources (other IDisposables) here
        _bar.Dispose();

        return true;
    }
}
```
