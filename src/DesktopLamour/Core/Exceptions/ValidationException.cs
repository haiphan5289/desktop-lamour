// ValidationException.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Exceptions;

public class ValidationException : Exception
{
    public string Field { get; }

    public ValidationException(string field, string message) : base(message)
        => Field = field;
}
