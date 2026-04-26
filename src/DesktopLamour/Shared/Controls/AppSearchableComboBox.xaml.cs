// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLamour.Shared.Controls;

public partial class AppSearchableComboBox : UserControl
{
    // ─── Dependency Properties ────────────────────────────────────────────────

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable),
            typeof(AppSearchableComboBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(ISearchableItem),
            typeof(AppSearchableComboBox),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(AppSearchableComboBox),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public static readonly DependencyProperty IsNullableProperty =
        DependencyProperty.Register(nameof(IsNullable), typeof(bool),
            typeof(AppSearchableComboBox),
            new PropertyMetadata(false));

    // ─── Public API ───────────────────────────────────────────────────────────

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ISearchableItem? SelectedItem
    {
        get => (ISearchableItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsNullable
    {
        get => (bool)GetValue(IsNullableProperty);
        set => SetValue(IsNullableProperty, value);
    }

    // ─── Internal ─────────────────────────────────────────────────────────────

    private readonly ObservableCollection<ISearchableItem> _filtered = new();
    private bool _suppressTextChange;

    public AppSearchableComboBox()
    {
        InitializeComponent();

        ItemsList.ItemsSource      = _filtered;
        ItemsList.DisplayMemberPath = nameof(ISearchableItem.DisplayText);

        SearchBox.GotFocus         += OnSearchGotFocus;
        SearchBox.LostFocus        += OnSearchLostFocus;
        SearchBox.TextChanged      += OnSearchTextChanged;
        ItemsList.PreviewMouseDown += OnListPreviewMouseDown;
        ToggleButton.Click         += OnToggleClick;
        ClearButton.Click          += OnClearClick;
    }

    // ─── DP Callbacks ─────────────────────────────────────────────────────────

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppSearchableComboBox combo)
            combo.PopulateFiltered(combo.SearchBox?.Text ?? string.Empty);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AppSearchableComboBox combo) return;
        combo._suppressTextChange = true;
        if (combo.SearchBox is not null)
            combo.SearchBox.Text = (e.NewValue as ISearchableItem)?.DisplayText ?? string.Empty;
        combo._suppressTextChange = false;
        combo.UpdatePlaceholder();
        combo.UpdateClearButton();
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AppSearchableComboBox combo && combo.PlaceholderText is not null)
        {
            combo.PlaceholderText.Text = e.NewValue as string ?? string.Empty;
            combo.UpdatePlaceholder();
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

    private void OnSearchLostFocus(object sender, RoutedEventArgs e)
    {
        FieldBorder.BorderBrush = (Brush)FindResource("AppColor.BorderRegular");
        // Restore display text if user typed without selecting
        if (SelectedItem is not null && SearchBox.Text != SelectedItem.DisplayText)
        {
            _suppressTextChange = true;
            SearchBox.Text      = SelectedItem.DisplayText;
            _suppressTextChange = false;
        }
        DropdownPopup.IsOpen = false;
        UpdatePlaceholder();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChange) return;

        // Clear selection when user edits text manually
        if (SelectedItem is not null)
        {
            _suppressTextChange = true;
            SelectedItem        = null;
            _suppressTextChange = false;
            UpdateClearButton();
        }

        PopulateFiltered(SearchBox.Text);
        DropdownPopup.IsOpen = _filtered.Count > 0;
        UpdatePlaceholder();
    }

    private void OnListPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Walk visual tree to find the clicked ListBoxItem
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element != ItemsList)
        {
            if (element is ListBoxItem lbi && lbi.DataContext is ISearchableItem item)
            {
                SelectItem(item);
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

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        _suppressTextChange      = true;
        SearchBox.Text           = string.Empty;
        SelectedItem             = null;
        _suppressTextChange      = false;
        DropdownPopup.IsOpen     = false;
        UpdatePlaceholder();
        UpdateClearButton();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SelectItem(ISearchableItem item)
    {
        _suppressTextChange  = true;
        SelectedItem         = item;
        SearchBox.Text       = item.DisplayText;
        _suppressTextChange  = false;
        DropdownPopup.IsOpen = false;
        UpdatePlaceholder();
        UpdateClearButton();
    }

    private void PopulateFiltered(string query)
    {
        _filtered.Clear();
        if (ItemsSource is null) return;

        var term = query.Trim();
        foreach (var obj in ItemsSource)
        {
            if (obj is not ISearchableItem item) continue;
            if (string.IsNullOrEmpty(term)
                || item.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                _filtered.Add(item);
            }
        }
    }

    private void UpdatePlaceholder()
    {
        PlaceholderText.Visibility =
            string.IsNullOrEmpty(SearchBox.Text) && SelectedItem is null
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void UpdateClearButton()
    {
        if (!IsNullable) return;
        var hasSelection        = SelectedItem is not null;
        ClearButton.Visibility  = hasSelection ? Visibility.Visible  : Visibility.Collapsed;
        ToggleButton.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
    }
}
