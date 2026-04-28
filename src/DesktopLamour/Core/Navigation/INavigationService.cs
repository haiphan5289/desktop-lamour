// INavigationService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Navigation;

public interface INavigationService
{
    void NavigateTo(string viewName);
    void NavigateTo(string viewName, object parameter);
    void GoBack();
    void NavigateToHome();
    bool CanGoBack { get; }
}
