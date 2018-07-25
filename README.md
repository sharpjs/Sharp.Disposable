# Sharp.Disposable

A thread-safe implementation of the [.NET Dispose Pattern](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern), plus extras.

## Status

**Private Beta Testing.**

## Overview

This package provides the following types in the `Sharp.Disposable` namespace:

Name             | Description
-----------------|------------
`Disposable`     | Base class for any disposable object.<br>– Thread-safe; dispose from any thread<br>– Guarantees that disposal happens only once.<br>– `public IsDisposed` property<br>– `protected RequireNotDisposed()` helper method
`DisposablePool` | Collects multiple disposable objects, disposing them when the pool itself is disposed.
`DisposableBox`  | Generic, mutable box that can hold a disposable object, ensuring the object gets disposed when another object is placed in the box or when the box itself is disposed.
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
