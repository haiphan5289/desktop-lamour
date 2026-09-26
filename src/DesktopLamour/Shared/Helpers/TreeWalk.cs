// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DesktopLamour.Shared.Helpers;

// 2026-09-26: dò ngược cây giao diện AN TOÀN. e.OriginalSource của sự kiện chuột có thể là phần tử chữ
// (Run/Hyperlink — ContentElement, KHÔNG phải Visual); gọi thẳng VisualTreeHelper.GetParent trên nó
// ném InvalidOperationException ("... is not a Visual or Visual3D") → app hiện "Lỗi ứng dụng". Hàm
// này tự chọn VisualTreeHelper hay LogicalTreeHelper tuỳ loại phần tử.
public static class TreeWalk
{
    public static DependencyObject? GetParent(DependencyObject? current) => current switch
    {
        null                  => null,
        Visual or Visual3D    => VisualTreeHelper.GetParent(current),
        _                     => LogicalTreeHelper.GetParent(current),
    };

    public static T? FindAncestorOrSelf<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = GetParent(current);
        }
        return null;
    }
}
