// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Shared.Controls;

public interface ISearchableItem
{
    int    Id          { get; }
    string Code        { get; }
    string Name        { get; }
    string DisplayText { get; }
}
