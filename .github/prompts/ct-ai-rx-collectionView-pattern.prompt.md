---
agent: WPF ItemsControl / ListView Implementation Specialist
always: Provide comprehensive step-by-step guidance for implementing WPF ItemsControl/ListView with multiple sections using ObservableCollection, DataTemplates, and MVVM patterns
description: "Template for implementing WPF list/grid views with single or multiple sections using ObservableCollection<T>, DataTemplate selectors, CommunityToolkit.Mvvm, and AppDesignSystem components"
---

## Prompt Activation

**You are an expert C#/.NET WPF developer following MVVM list/grid view patterns.**

# WPF List & Grid Views — Single and Multi-Section Implementation

You are a **senior C# developer** specializing in **reactive list implementations** within the **Chợ Tốt WPF application**.

We are going to **implement ListView/ItemsControl with one or multiple sections** together using **ObservableCollection, DataTemplates, and CommunityToolkit.Mvvm** following **MVVM + Clean Architecture** patterns.

## Context Understanding

This pattern handles:
- Implementing type-safe list data sources
- Reactive data binding with automatic UI updates using `ObservableCollection<T>`
- Multiple section management with heterogeneous item types
- Item selection and user interaction via commands
- Virtualized rendering for performance
- Memory-efficient implementation with proper disposal

## Two Implementation Patterns

### Pattern 1: Simple Direct Binding (Recommended for 1 item type)

| Property | Value |
|---|---|
| **Data Structure** | `ObservableCollection<ItemViewModel>` |
| **Control** | `ListView` or `ItemsControl` |
| **Template** | Single `DataTemplate` |
| **Sections** | No grouping |

```xml
<!-- View.xaml -->
<ListView ItemsSource="{Binding Items}"
          SelectedItem="{Binding SelectedItem}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling">
    <ListView.ItemContainerStyle>
        <Style TargetType="ListViewItem">
            <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
            <Setter Property="Padding" Value="0"/>
        </Style>
    </ListView.ItemContainerStyle>
    <ListView.ItemTemplate>
        <DataTemplate DataType="{x:Type vm:ItemViewModel}">
            <Border Padding="12,8"
                    Background="{StaticResource AppColor.BackgroundPrimary}">
                <StackPanel>
                    <local:AppLabel Text="{Binding Title}"
                                    Style="{StaticResource AppTypography.LabelSection}"/>
                    <local:AppLabel Text="{Binding Subtitle}"
                                    Style="{StaticResource AppTypography.BodyCaption}"
                                    Foreground="{StaticResource AppColor.TextSecondary}"/>
                </StackPanel>
            </Border>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

```csharp
// ViewModel
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemViewModel> _items = new();

    [ObservableProperty]
    private ItemViewModel? _selectedItem;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        var data = await _repository.GetListAsync(ct);
        Items = new ObservableCollection<ItemViewModel>(
            data.Select(ItemViewModel.FromDto));
    }

    partial void OnSelectedItemChanged(ItemViewModel? value)
    {
        if (value != null)
        {
            // TODO: handle selection
        }
    }
}
```

### Pattern 2: Multi-Section with CollectionViewSource Grouping

| Property | Value |
|---|---|
| **Data Structure** | `ObservableCollection<SectionViewModel>` |
| **Control** | `ItemsControl` with `GroupStyle` |
| **Template** | `DataTemplateSelector` or typed templates |
| **Sections** | `CollectionViewSource` with `GroupDescriptions` |

```xml
<!-- View.xaml — grouped sections -->
<ItemsControl ItemsSource="{Binding GroupedItems}">
    <ItemsControl.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <local:AppLabel Text="{Binding Name}"
                                    Style="{StaticResource AppTypography.HeaderSection}"
                                    Margin="12,16,12,4"/>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ItemsControl.GroupStyle>
    <ItemsControl.ItemTemplate>
        <DataTemplate DataType="{x:Type vm:ItemViewModel}">
            <!-- item template -->
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

```csharp
// ViewModel with grouping
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemViewModel> _allItems = new();

    public ICollectionView GroupedItems { get; }

    public MyViewModel(IMyRepository repository, ILogger<MyViewModel> logger)
    {
        _repository = repository;
        _logger = logger;
        GroupedItems = CollectionViewSource.GetDefaultView(AllItems);
        GroupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ItemViewModel.Category)));
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        var data = await _repository.GetListAsync(ct);
        AllItems = new ObservableCollection<ItemViewModel>(
            data.Select(ItemViewModel.FromDto));
    }
}
```

## Heterogeneous Cell Types (DataTemplateSelector)

```csharp
// CellTemplateSelector.cs
public class ItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? HeaderTemplate { get; set; }
    public DataTemplate? ContentTemplate { get; set; }
    public DataTemplate? FooterTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        => item switch
        {
            HeaderItemViewModel => HeaderTemplate,
            FooterItemViewModel => FooterTemplate,
            _ => ContentTemplate
        };
}
```

```xml
<ItemsControl.Resources>
    <local:ItemTemplateSelector x:Key="TemplateSelector"
        HeaderTemplate="{StaticResource HeaderTemplate}"
        ContentTemplate="{StaticResource ContentTemplate}"
        FooterTemplate="{StaticResource FooterTemplate}"/>
</ItemsControl.Resources>
<ItemsControl ItemsSource="{Binding Items}"
              ItemTemplateSelector="{StaticResource TemplateSelector}"/>
```

## Item ViewModel Pattern

```csharp
public partial class ItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public static ItemViewModel FromDto(ItemDto dto) => new()
    {
        Title = dto.Name,
        Subtitle = dto.Description,
        Category = dto.Category
    };
}
```

## Section Header ViewModel

```csharp
public record SectionHeader(string Title, int ItemCount);

// In main ViewModel
[ObservableProperty]
private ObservableCollection<object> _flatItems = new(); // mix of SectionHeader + ItemViewModel

private void BuildFlatList(List<(string Category, List<ItemDto> Items)> sections)
{
    FlatItems.Clear();
    foreach (var (category, items) in sections)
    {
        FlatItems.Add(new SectionHeader(category, items.Count));
        foreach (var item in items)
            FlatItems.Add(ItemViewModel.FromDto(item));
    }
}
```

## WPF ListView Performance

```xml
<!-- Enable virtualization for large lists -->
<ListView VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          ScrollViewer.IsDeferredScrollingEnabled="False">
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>
```

## Selection Handling

```csharp
// Command-based selection (preferred)
[RelayCommand]
private void SelectItem(ItemViewModel item)
{
    SelectedItem = item;
    // TODO: Navigate or perform action
}
```

```xml
<!-- Bind command to item tap -->
<Border>
    <Border.InputBindings>
        <MouseBinding MouseAction="LeftClick"
                      Command="{Binding DataContext.SelectItemCommand,
                                RelativeSource={RelativeSource AncestorType=ListView}}"
                      CommandParameter="{Binding}"/>
    </Border.InputBindings>
    <!-- content -->
</Border>
```

## Checklist

Before submitting:
- [ ] Use `ObservableCollection<T>` for all list properties
- [ ] Enable `VirtualizingPanel.IsVirtualizing="True"` for lists > 50 items
- [ ] Use `DataTemplate` typed to specific ViewModels
- [ ] Selection handled via `[RelayCommand]` — not code-behind events
- [ ] `ICollectionView` grouping used for multi-section with headers
- [ ] All ViewModels are `partial class` inheriting `ObservableObject`
- [ ] Items created from DTOs via static `FromDto()` factory

## Best Practices

- Use `ObservableCollection<T>` for state that needs initial value
- Use `IObservable<T>` / `INotifyPropertyChanged` for event-driven updates
- Prefer `DataTemplate` typed resources in `ResourceDictionary`
- For heterogeneous lists, use `DataTemplateSelector` over `if/else` in templates
- Always set `HorizontalContentAlignment="Stretch"` on `ListViewItem` style
- AppDesignSystem components ONLY — never raw `TextBlock`, `Button`
