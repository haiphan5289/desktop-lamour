---
description: "Generate a WPF DataTemplate or DataGrid row template for Desktop Lamour item lists."
mode: "agent"
---

# DataTemplate / Row Template Generator — Desktop Lamour

## Input

```
MODULE:         <Employees>
ITEM_MODEL:     <Employee>
CONTAINER_TYPE: <DataGrid | ListBox | ItemsControl>
FIELDS:         <Name, Phone, Role>
```

## DataGrid Template

```xml
<DataGrid ItemsSource="{Binding Items}"
          AutoGenerateColumns="False" IsReadOnly="True"
          Background="{StaticResource AppColor.BackgroundPrimary}"
          BorderThickness="0">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
        <DataGridTextColumn Header="Phone" Binding="{Binding Phone}" Width="Auto"/>
        <DataGridTemplateColumn Header="Actions" Width="Auto">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <controls:AppButton Content="Edit"
                        Style="{StaticResource AppButton.Tertiary.Medium}"
                        Command="{Binding DataContext.EditCommand,
                                  RelativeSource={RelativeSource AncestorType=DataGrid}}"
                        CommandParameter="{Binding}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

## ListBox ItemTemplate

```xml
<ListBox ItemsSource="{Binding Items}"
         Background="{StaticResource AppColor.BackgroundPrimary}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <Border Padding="{StaticResource AppSpacing.Medium}"
                    BorderBrush="{StaticResource AppColor.BorderThin}"
                    BorderThickness="0,0,0,1">
                <StackPanel>
                    <controls:AppLabel Text="{Binding Name}"
                                       Style="{StaticResource AppTypography.LabelSection}"/>
                    <controls:AppLabel Text="{Binding Phone}"
                                       Style="{StaticResource AppTypography.BodyCaption}"/>
                </StackPanel>
            </Border>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```
