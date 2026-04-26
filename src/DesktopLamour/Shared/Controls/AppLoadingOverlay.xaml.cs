using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

public partial class AppLoadingOverlay : UserControl
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AppLoadingOverlay),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public AppLoadingOverlay()
    {
        InitializeComponent();
    }
}
