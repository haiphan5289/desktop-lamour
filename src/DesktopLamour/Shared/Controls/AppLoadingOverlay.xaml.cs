using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DesktopLamour.Shared.Controls;

public partial class AppLoadingOverlay : UserControl
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AppLoadingOverlay),
            new PropertyMetadata(false, OnIsLoadingChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    // 2026-11-19 (perf): spinner chỉ quay khi THẬT SỰ đang tải và overlay đang nằm trong visual tree.
    // Trước đây Storyboard RepeatBehavior=Forever bắt đầu từ Loaded và không bao giờ dừng — overlay
    // Collapsed vẫn quay ngầm, dùng ở ~45 màn nên càng mở nhiều màn/popup càng nặng (render loop
    // chạy liên tục, rõ nhất khi điều khiển qua UltraViewer/VM vì WPF phải vẽ bằng CPU).
    private static readonly DoubleAnimation SpinAnimation = CreateSpinAnimation();

    public AppLoadingOverlay()
    {
        InitializeComponent();
        Loaded   += (_, _) => UpdateSpinner();
        Unloaded += (_, _) => StopSpinner();
    }

    private static DoubleAnimation CreateSpinAnimation()
    {
        var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(0.85))
        {
            RepeatBehavior = RepeatBehavior.Forever,
        };
        animation.Freeze();
        return animation;
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((AppLoadingOverlay)d).UpdateSpinner();

    private void UpdateSpinner()
    {
        if (IsLoading && IsLoaded)
            SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, SpinAnimation);
        else
            StopSpinner();
    }

    // BeginAnimation(..., null) gỡ hẳn AnimationClock khỏi property → không còn tick nữa.
    private void StopSpinner() => SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, null);
}
