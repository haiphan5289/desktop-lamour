// ViewModelBase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Core.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    private static readonly HashSet<string> _noDirtyProps = new()
    {
        nameof(IsDirty), "IsLoading", "HasError", "ErrorMessage", "WindowTitle",
        "IsAddMode", "IsEditMode", "TotalAmount", "SelectedReceiptType"
    };

    private bool _dirtyTracking;

    [ObservableProperty] private bool _isDirty;

    protected void BeginDirtyTracking() { _dirtyTracking = true; IsDirty = false; }
    protected void StopDirtyTracking()  { _dirtyTracking = false; IsDirty = false; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_dirtyTracking && !_noDirtyProps.Contains(e.PropertyName!))
            IsDirty = true;
    }
}
