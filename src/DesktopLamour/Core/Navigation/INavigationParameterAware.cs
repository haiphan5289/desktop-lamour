// INavigationParameterAware.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Navigation;

public interface INavigationParameterAware
{
    void OnNavigatedTo(object? parameter);
}
