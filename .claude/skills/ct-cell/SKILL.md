---
name: ct-cell
description: Generate a WPF DataTemplate or DataGrid row template for Desktop Lamour. Creates XAML DataTemplate for ItemsControl/ListBox/DataGrid with proper binding, style references from AppStyles.xaml, and a C# item ViewModel class. Use when displaying a list of items in any container type.
model: haiku
effort: low
---

# WPF DataTemplate / Row Template Generator

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate WPF DataTemplates and item ViewModels for Desktop Lamour list/grid containers.

## Input Format

```
TEMPLATE_NAME: <Name, e.g. "EmployeeCard">
ENTITY: <domain entity, e.g. "Employee">
BINDINGS: <comma-separated field names, e.g. "FullName, PhoneNumber, Role, IsActive">
CONTAINER_TYPE: <DataGrid | ListBox | ItemsControl>
```

---

## DataGrid Column Template

For `CONTAINER_TYPE: DataGrid`:

```xml
<!-- In [Name]View.xaml — DataGrid with typed columns -->
<DataGrid ItemsSource="{Binding Employees}"
          AutoGenerateColumns="False"
          IsReadOnly="True"
          Style="{StaticResource DataGridStyle}"
          RowStyle="{StaticResource DataGridRowStyle}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Họ tên"
                            Binding="{Binding FullName}"
                            Width="*"/>
        <DataGridTextColumn Header="Điện thoại"
                            Binding="{Binding PhoneNumber}"
                            Width="150"/>
        <DataGridTextColumn Header="Chức vụ"
                            Binding="{Binding Role}"
                            Width="120"/>
        <DataGridCheckBoxColumn Header="Hoạt động"
                                Binding="{Binding IsActive}"
                                Width="100"/>
        <!-- Action column -->
        <DataGridTemplateColumn Header="" Width="80">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button Content="Xoá"
                            Style="{StaticResource ButtonDangerStyle}"
                            Command="{Binding DataContext.DeleteCommand,
                                      RelativeSource={RelativeSource AncestorType=DataGrid}}"
                            CommandParameter="{Binding Id}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

---

## ListBox DataTemplate

For `CONTAINER_TYPE: ListBox`:

```xml
<!-- In [Name]View.xaml -->
<ListBox ItemsSource="{Binding Items}"
         SelectedItem="{Binding SelectedItem}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <Border Style="{StaticResource CardBorderStyle}" Margin="0,4">
                <Grid Margin="12,8">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0">
                        <TextBlock Text="{Binding FullName}"
                                   Style="{StaticResource TextSubheadingStyle}"/>
                        <TextBlock Text="{Binding PhoneNumber}"
                                   Style="{StaticResource TextBodyStyle}"/>
                    </StackPanel>

                    <TextBlock Grid.Column="1"
                               Text="{Binding Role}"
                               Style="{StaticResource TextCaptionStyle}"
                               VerticalAlignment="Center"/>
                </Grid>
            </Border>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

---

## ItemsControl DataTemplate

For `CONTAINER_TYPE: ItemsControl` (read-only list, no selection):

```xml
<!-- In [Name]View.xaml -->
<ItemsControl ItemsSource="{Binding Items}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Style="{StaticResource CardBorderStyle}" Margin="0,4">
                <Grid Margin="12,8">
                    <TextBlock Text="{Binding FullName}"
                               Style="{StaticResource TextBodyStyle}"/>
                </Grid>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

---

## Item ViewModel Class (C#)

When binding complex objects, define a dedicated item ViewModel:

```csharp
// File: src/DesktopLamour/Features/[Module]/ViewModels/[Name]ItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Features.[Module].ViewModels;

public partial class [Name]ItemViewModel : ObservableObject
{
    public int Id { get; init; }

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private bool _isActive;

    // Factory method from domain model
    public static [Name]ItemViewModel From([Entity] entity) => new()
    {
        Id = entity.Id,
        FullName = entity.FullName,
        PhoneNumber = entity.PhoneNumber,
        Role = entity.Role,
        IsActive = entity.IsActive
    };
}
```

---

## Binding to Parent ViewModel Commands

When a DataTemplate button needs to call a parent ViewModel command, use `RelativeSource`:

```xml
<Button Command="{Binding DataContext.DeleteEmployeeCommand,
                  RelativeSource={RelativeSource AncestorType=UserControl}}"
        CommandParameter="{Binding Id}"
        Content="Xoá"
        Style="{StaticResource ButtonDangerStyle}"/>
```

---

## Rules

- Always reference style keys via `StaticResource` — never hardcode colors or fonts
- Verify style keys exist in `AppStyles.xaml` before using them
- Use `RelativeSource AncestorType` to reach parent ViewModel commands from DataTemplate
- Prefer `DataGrid` for tabular data (employees, products, invoice lines)
- Prefer `ListBox` for card-style items with selection
- Prefer `ItemsControl` for read-only scrollable lists
