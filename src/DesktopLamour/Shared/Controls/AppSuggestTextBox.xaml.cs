// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLamour.Shared.Controls;

// Giống AppSearchableComboBox (search + gợi ý dropdown) nhưng KHÔNG ép Text phải khớp 1 item —
// dùng cho các field cần gợi ý nhưng vẫn cho gõ tự do (ví dụ "Tên Khách hàng" trong Chứng từ bán
// hàng: gợi ý theo tên khách hàng thật, nhưng cho phép ghi đè bằng text tuỳ ý mà không bị
// AppSearchableComboBox tự trả về tên cũ khi rời ô — xem OnSearchLostFocus).
public partial class AppSuggestTextBox : UserControl
{
    // ─── Dependency Properties ────────────────────────────────────────────────

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(AppSuggestTextBox),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    public static readonly DependencyProperty SuggestionsProperty =
        DependencyProperty.Register(nameof(Suggestions), typeof(IEnumerable),
            typeof(AppSuggestTextBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(AppSuggestTextBox),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public static readonly DependencyProperty SuggestionPickedCommandProperty =
        DependencyProperty.Register(nameof(SuggestionPickedCommand), typeof(ICommand),
            typeof(AppSuggestTextBox),
            new PropertyMetadata(null));

    // ─── Public API ───────────────────────────────────────────────────────────

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IEnumerable? Suggestions
    {
        get => (IEnumerable?)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // Thực thi khi user bấm chọn 1 gợi ý trong dropdown — CommandParameter là ISearchableItem đã
    // chọn, cho phép ViewModel đồng bộ lại field khác (ví dụ set lại SelectedCustomer/CustomerId).
    public ICommand? SuggestionPickedCommand
    {
        get => (ICommand?)GetValue(SuggestionPickedCommandProperty);
        set => SetValue(SuggestionPickedCommandProperty, value);
    }

    // ─── Internal ─────────────────────────────────────────────────────────────

    private readonly ObservableCollection<ISearchableItem> _filtered = new();
    private bool _suppressTextChange;

    public AppSuggestTextBox()
    {
        InitializeComponent();

        ItemsList.ItemsSource      = _filtered;

        SearchBox.GotFocus         += OnSearchGotFocus;
        SearchBox.LostFocus        += OnSearchLostFocus;
        SearchBox.TextChanged      += OnSearchTextChanged;
        ItemsList.PreviewMouseDown += OnListPreviewMouseDown;
        ToggleButton.Click         += OnToggleClick;
    }

    // ─── DP Callbacks ─────────────────────────────────────────────────────────

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppSuggestTextBox box) return;
        var newText = e.NewValue as string ?? string.Empty;
        if (box.SearchBox.Text == newText) return;

        box._suppressTextChange = true;
        box.SearchBox.Text      = newText;
        box._suppressTextChange = false;
        box.UpdatePlaceholder();
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppSuggestTextBox box && box.PlaceholderText is not null)
        {
            box.PlaceholderText.Text = e.NewValue as string ?? string.Empty;
            box.UpdatePlaceholder();
        }
    }

    // ─── Event Handlers ───────────────────────────────────────────────────────

    private void OnSearchGotFocus(object sender, RoutedEventArgs e)
    {
        FieldBorder.BorderBrush = (Brush)FindResource("AppColor.BorderActive");
        PopulateFiltered(SearchBox.Text);
        if (_filtered.Count > 0)
            DropdownPopup.IsOpen = true;
    }

    // KHÁC AppSearchableComboBox: rời ô KHÔNG trả Text về gợi ý cũ — giữ nguyên đúng text user
    // vừa gõ, dù không khớp gợi ý nào (đây chính là mục đích tồn tại của control này).
    private void OnSearchLostFocus(object sender, RoutedEventArgs e)
    {
        FieldBorder.BorderBrush = (Brush)FindResource("AppColor.BorderRegular");
        DropdownPopup.IsOpen    = false;
        UpdatePlaceholder();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChange) return;

        _suppressTextChange = true;
        Text                = SearchBox.Text;
        _suppressTextChange = false;

        PopulateFiltered(SearchBox.Text);
        DropdownPopup.IsOpen = _filtered.Count > 0;
        UpdatePlaceholder();
    }

    private void OnListPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element != ItemsList)
        {
            if (element is ListBoxItem lbi && lbi.DataContext is ISearchableItem item)
            {
                PickSuggestion(item);
                e.Handled = true;
                return;
            }
            element = VisualTreeHelper.GetParent(element);
        }
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        if (DropdownPopup.IsOpen)
        {
            DropdownPopup.IsOpen = false;
        }
        else
        {
            PopulateFiltered(SearchBox.Text);
            if (_filtered.Count > 0)
            {
                DropdownPopup.IsOpen = true;
                SearchBox.Focus();
            }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void PickSuggestion(ISearchableItem item)
    {
        _suppressTextChange  = true;
        SearchBox.Text       = item.Name;
        _suppressTextChange  = false;
        Text                 = item.Name;
        DropdownPopup.IsOpen  = false;
        UpdatePlaceholder();
        SuggestionPickedCommand?.Execute(item);
    }

    // Chỉ lọc theo Name (không Code/Phone) — control này chuyên cho field "gợi ý theo tên".
    // Text trống khớp tất cả (giống AppSearchableComboBox) — focus vào ô trống hiện cả danh sách.
    private void PopulateFiltered(string query)
    {
        _filtered.Clear();
        if (Suggestions is null) return;

        var term = query.Trim();
        foreach (var obj in Suggestions)
        {
            if (obj is not ISearchableItem item) continue;
            if (term.Length == 0 || item.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                _filtered.Add(item);
        }
    }

    private void UpdatePlaceholder()
    {
        PlaceholderText.Visibility =
            string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }
}
