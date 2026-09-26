// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Shared.Helpers;

/// <summary>
/// 2026-09-26: tô đỏ ô số khi giá trị âm — khớp MISA ("(1.458.000)" đỏ). Gắn
/// <c>helpers:NegativeAmountHighlight.IsEnabled="True"</c> lên TextBlock / AppLabel (hoặc Setter
/// trong DataGridTextColumn.ElementStyle). Tự bind vào chính Text của phần tử nên không cần biết
/// ô đó hiển thị property nào; khi hết âm thì trả lại đúng màu chữ gốc (kể cả màu set sẵn trong XAML).
/// </summary>
public static class NegativeAmountHighlight
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(NegativeAmountHighlight),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject d) => (bool)d.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject d, bool value) => d.SetValue(IsEnabledProperty, value);

    // Bản sao Text của phần tử (qua binding RelativeSource Self) — đổi Text ⇒ đánh giá lại màu.
    private static readonly DependencyProperty WatchedTextProperty =
        DependencyProperty.RegisterAttached(
            "WatchedText", typeof(string), typeof(NegativeAmountHighlight),
            new PropertyMetadata(null, OnWatchedTextChanged));

    private static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.RegisterAttached(
            "IsHighlighted", typeof(bool), typeof(NegativeAmountHighlight), new PropertyMetadata(false));

    // Màu chữ LOCAL gốc trước khi tô đỏ — để trả lại đúng khi hết âm. WPF cấm dùng UnsetValue làm
    // default của DependencyProperty (ném ArgumentException ngay khi class được nạp → mọi màn có ô số
    // crash), nên "không có màu local" được lưu bằng sentinel NoLocalForeground thay vì UnsetValue.
    private static readonly object NoLocalForeground = new();

    private static readonly DependencyProperty SavedForegroundProperty =
        DependencyProperty.RegisterAttached(
            "SavedForeground", typeof(object), typeof(NegativeAmountHighlight),
            new PropertyMetadata(null));

    private static readonly Brush FallbackRed = CreateFallbackRed();

    private static Brush CreateFallbackRed()
    {
        var brush = new SolidColorBrush(Color.FromRgb(0xD0, 0x02, 0x1B)); // = AppColor.TextError
        brush.Freeze();
        return brush;
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textProperty = d switch
        {
            TextBlock => TextBlock.TextProperty,
            AppLabel  => AppLabel.TextProperty,
            _ => null,
        };
        if (textProperty is null) return;

        if (e.NewValue is true)
            BindingOperations.SetBinding(d, WatchedTextProperty,
                new Binding(textProperty.Name) { RelativeSource = RelativeSource.Self, Mode = BindingMode.OneWay });
        else
        {
            BindingOperations.ClearBinding(d, WatchedTextProperty);
            Apply(d, false);
        }
    }

    private static void OnWatchedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => Apply(d, MoneyFormat.IsNegativeText(e.NewValue as string));

    private static void Apply(DependencyObject d, bool negative)
    {
        var wasHighlighted = (bool)d.GetValue(IsHighlightedProperty);
        if (negative == wasHighlighted) return;
        d.SetValue(IsHighlightedProperty, negative);

        if (negative)
        {
            var local = d.ReadLocalValue(TextElement.ForegroundProperty);
            // Màu chữ đang bind động (BindingExpression) — không đè để khỏi phá binding gốc.
            if (local is BindingExpressionBase) { d.SetValue(IsHighlightedProperty, false); return; }
            d.SetValue(SavedForegroundProperty, local == DependencyProperty.UnsetValue ? NoLocalForeground : local);
            var red = (d as FrameworkElement)?.TryFindResource("AppColor.TextError") as Brush ?? FallbackRed;
            d.SetValue(TextElement.ForegroundProperty, red);
        }
        else
        {
            var saved = d.GetValue(SavedForegroundProperty);
            if (saved is null || ReferenceEquals(saved, NoLocalForeground)) d.ClearValue(TextElement.ForegroundProperty);
            else d.SetValue(TextElement.ForegroundProperty, saved);
            d.ClearValue(SavedForegroundProperty);
        }
    }
}
