// MainWindowViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _currentContent;
}
