// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Controls;

// DropdownText on ISearchableItem is a default interface member — WPF's DisplayMemberPath
// resolves properties via reflection on the runtime type, which does not see default
// interface implementations unless the concrete class overrides them. Converting through
// plain C# (interface dispatch) here sidesteps that limitation.
public class SearchableItemDropdownTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ISearchableItem item ? item.DropdownText : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
