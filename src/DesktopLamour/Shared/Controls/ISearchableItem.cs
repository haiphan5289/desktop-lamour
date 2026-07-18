// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Shared.Controls;

public interface ISearchableItem
{
    int     Id          { get; }
    string  Code        { get; }
    string  Name        { get; }
    string  DisplayText { get; }
    string? Phone => null;

    // Text shown in the dropdown list only — DisplayText (used for the input box after selection) is unchanged.
    string DropdownText => Phone is { Length: > 0 } ? $"{DisplayText} — {Phone}" : DisplayText;
}
