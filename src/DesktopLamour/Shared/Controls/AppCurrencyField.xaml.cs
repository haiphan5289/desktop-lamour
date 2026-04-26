// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLamour.Shared.Controls;

public partial class AppCurrencyField : UserControl
{
    private static readonly CultureInfo ViVn = new("vi-VN");

    // ─── Dependency Properties ────────────────────────────────────────────────

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(decimal),
            typeof(AppCurrencyField),
            new FrameworkPropertyMetadata(0m,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(AppCurrencyField),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // ─── Internal ─────────────────────────────────────────────────────────────

    private bool   _updating;
    private string _digits = string.Empty;

    public AppCurrencyField()
    {
        InitializeComponent();

        InputBox.PreviewTextInput += OnPreviewTextInput;
        InputBox.PreviewKeyDown   += OnPreviewKeyDown;
        InputBox.TextChanged      += OnTextChanged;
        InputBox.GotFocus         += OnGotFocus;
        InputBox.LostFocus        += OnLostFocus;

        DataObject.AddPastingHandler(InputBox, OnPaste);
    }

    // ─── DP Callbacks ─────────────────────────────────────────────────────────

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppCurrencyField f || f._updating) return;
        var val    = (decimal)e.NewValue;
        f._digits  = val > 0 ? ((long)val).ToString() : string.Empty;
        f._updating = true;
        f.InputBox.Text = Format(val);
        f._updating = false;
        f.UpdatePlaceholder();
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppCurrencyField f)
        {
            f.PlaceholderBlock.Text = e.NewValue as string ?? string.Empty;
            f.UpdatePlaceholder();
        }
    }

    // ─── Event Handlers ───────────────────────────────────────────────────────

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Skip over thousand-separator dots when pressing Backspace
        if (e.Key == Key.Back && InputBox.CaretIndex > 0
            && InputBox.SelectionLength == 0)
        {
            var idx = InputBox.CaretIndex;
            if (idx <= InputBox.Text.Length && InputBox.Text[idx - 1] == '.')
                InputBox.CaretIndex = idx - 1;
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updating) return;

        var newDigits = new string(InputBox.Text.Where(char.IsDigit).ToArray());
        if (newDigits == _digits) return;

        _digits  = newDigits;
        var val  = long.TryParse(_digits, out var v) ? (decimal)v : 0m;

        _updating       = true;
        Value           = val;
        InputBox.Text   = Format(val);
        InputBox.CaretIndex = InputBox.Text.Length;
        _updating       = false;

        UpdatePlaceholder();
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        FieldBorder.BorderBrush = (Brush)FindResource("AppColor.BorderActive");
        InputBox.SelectAll();
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        FieldBorder.BorderBrush = (Brush)FindResource("AppColor.BorderRegular");
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text   = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
            var digits = new string(text.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(digits))
            {
                e.DataObject = new DataObject(DataFormats.Text, digits);
                return;
            }
        }
        e.CancelCommand();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string Format(decimal val)
        => val > 0 ? val.ToString("N0", ViVn) : string.Empty;

    private void UpdatePlaceholder()
    {
        PlaceholderBlock.Visibility =
            string.IsNullOrEmpty(InputBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
