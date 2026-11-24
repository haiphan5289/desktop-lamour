// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DesktopLamour.Shared.Utilities;

/// <summary>
/// ObservableCollection có thêm <see cref="ReplaceAll"/> — thay toàn bộ nội dung rồi bắn ĐÚNG 1 sự kiện
/// Reset, thay vì Clear() + N lần Add() (mỗi Add là 1 CollectionChanged → ICollectionView chạy filter +
/// DataGrid/ListBox cập nhật 1 lần). Với vài nghìn dòng, cách cũ làm UI đứng vài giây.
/// </summary>
public class BulkObservableCollection<T> : ObservableCollection<T>
{
    public BulkObservableCollection() { }

    public BulkObservableCollection(IEnumerable<T> items) : base(items) { }

    public void ReplaceAll(IEnumerable<T> items)
    {
        CheckReentrancy();

        Items.Clear();
        foreach (var item in items)
            Items.Add(item);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
