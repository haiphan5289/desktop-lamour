// DocumentToolbar.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Thanh công cụ chứng từ dùng chung cho các popup: Bán hàng, Bán hàng trả lại,
/// Nhập kho, Phiếu thu, Phiếu chi. Window chỉ bind command nào nó thực sự có —
/// nút không có command tương ứng sẽ tự ẩn (xem DocumentToolbar.xaml).
/// Nút "Đóng" phát <see cref="CloseRequested"/> để window tự đóng.
/// </summary>
public partial class DocumentToolbar : UserControl
{
    public DocumentToolbar() => InitializeComponent();

    // ─── Command Dependency Properties ───────────────────────────────────────

    private static DependencyProperty Cmd(string name) =>
        DependencyProperty.Register(name, typeof(ICommand), typeof(DocumentToolbar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty PrevCommandProperty         = Cmd(nameof(PrevCommand));
    public static readonly DependencyProperty NextCommandProperty         = Cmd(nameof(NextCommand));
    public static readonly DependencyProperty AddCommandProperty          = Cmd(nameof(AddCommand));
    public static readonly DependencyProperty EditCommandProperty         = Cmd(nameof(EditCommand));
    public static readonly DependencyProperty DeleteCommandProperty       = Cmd(nameof(DeleteCommand));
    public static readonly DependencyProperty SaveCommandProperty         = Cmd(nameof(SaveCommand));
    public static readonly DependencyProperty UnpostCommandProperty       = Cmd(nameof(UnpostCommand));
    public static readonly DependencyProperty HoldCommandProperty         = Cmd(nameof(HoldCommand));
    public static readonly DependencyProperty CreateExportCommandProperty = Cmd(nameof(CreateExportCommand));
    public static readonly DependencyProperty PrintCommandProperty        = Cmd(nameof(PrintCommand));

    public ICommand? PrevCommand         { get => (ICommand?)GetValue(PrevCommandProperty);         set => SetValue(PrevCommandProperty, value); }
    public ICommand? NextCommand         { get => (ICommand?)GetValue(NextCommandProperty);         set => SetValue(NextCommandProperty, value); }
    public ICommand? AddCommand          { get => (ICommand?)GetValue(AddCommandProperty);          set => SetValue(AddCommandProperty, value); }
    public ICommand? EditCommand         { get => (ICommand?)GetValue(EditCommandProperty);         set => SetValue(EditCommandProperty, value); }
    public ICommand? DeleteCommand       { get => (ICommand?)GetValue(DeleteCommandProperty);       set => SetValue(DeleteCommandProperty, value); }
    public ICommand? SaveCommand         { get => (ICommand?)GetValue(SaveCommandProperty);         set => SetValue(SaveCommandProperty, value); }
    public ICommand? UnpostCommand       { get => (ICommand?)GetValue(UnpostCommandProperty);       set => SetValue(UnpostCommandProperty, value); }
    public ICommand? HoldCommand         { get => (ICommand?)GetValue(HoldCommandProperty);         set => SetValue(HoldCommandProperty, value); }
    public ICommand? CreateExportCommand { get => (ICommand?)GetValue(CreateExportCommandProperty); set => SetValue(CreateExportCommandProperty, value); }
    public ICommand? PrintCommand        { get => (ICommand?)GetValue(PrintCommandProperty);        set => SetValue(PrintCommandProperty, value); }

    // ─── Per-button visibility flags (mặc định Visible) ──────────────────────
    // Window bind cờ này ở đúng chỗ trước đây gate bằng Visibility="{Binding IsEditable...}"
    // để giữ nguyên hành vi (vd. chế độ chỉ xem của Chứng từ bán hàng).

    private static DependencyProperty Vis(string name) =>
        DependencyProperty.Register(name, typeof(Visibility), typeof(DocumentToolbar),
            new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty PrevVisibilityProperty         = Vis(nameof(PrevVisibility));
    public static readonly DependencyProperty NextVisibilityProperty         = Vis(nameof(NextVisibility));
    public static readonly DependencyProperty AddVisibilityProperty          = Vis(nameof(AddVisibility));
    public static readonly DependencyProperty EditVisibilityProperty         = Vis(nameof(EditVisibility));
    public static readonly DependencyProperty DeleteVisibilityProperty       = Vis(nameof(DeleteVisibility));
    public static readonly DependencyProperty SaveVisibilityProperty         = Vis(nameof(SaveVisibility));
    public static readonly DependencyProperty UnpostVisibilityProperty       = Vis(nameof(UnpostVisibility));
    public static readonly DependencyProperty HoldVisibilityProperty         = Vis(nameof(HoldVisibility));
    public static readonly DependencyProperty CreateExportVisibilityProperty = Vis(nameof(CreateExportVisibility));
    public static readonly DependencyProperty PrintVisibilityProperty        = Vis(nameof(PrintVisibility));

    public Visibility PrevVisibility         { get => (Visibility)GetValue(PrevVisibilityProperty);         set => SetValue(PrevVisibilityProperty, value); }
    public Visibility NextVisibility         { get => (Visibility)GetValue(NextVisibilityProperty);         set => SetValue(NextVisibilityProperty, value); }
    public Visibility AddVisibility          { get => (Visibility)GetValue(AddVisibilityProperty);          set => SetValue(AddVisibilityProperty, value); }
    public Visibility EditVisibility         { get => (Visibility)GetValue(EditVisibilityProperty);         set => SetValue(EditVisibilityProperty, value); }
    public Visibility DeleteVisibility       { get => (Visibility)GetValue(DeleteVisibilityProperty);       set => SetValue(DeleteVisibilityProperty, value); }
    public Visibility SaveVisibility         { get => (Visibility)GetValue(SaveVisibilityProperty);         set => SetValue(SaveVisibilityProperty, value); }
    public Visibility UnpostVisibility       { get => (Visibility)GetValue(UnpostVisibilityProperty);       set => SetValue(UnpostVisibilityProperty, value); }
    public Visibility HoldVisibility         { get => (Visibility)GetValue(HoldVisibilityProperty);         set => SetValue(HoldVisibilityProperty, value); }
    public Visibility CreateExportVisibility { get => (Visibility)GetValue(CreateExportVisibilityProperty); set => SetValue(CreateExportVisibilityProperty, value); }
    public Visibility PrintVisibility        { get => (Visibility)GetValue(PrintVisibilityProperty);        set => SetValue(PrintVisibilityProperty, value); }

    // ─── Label / toggle Dependency Properties ────────────────────────────────

    public static readonly DependencyProperty SaveLabelProperty =
        DependencyProperty.Register(nameof(SaveLabel), typeof(string), typeof(DocumentToolbar),
            new PropertyMetadata("Ghi sổ"));

    public static readonly DependencyProperty CreateExportLabelProperty =
        DependencyProperty.Register(nameof(CreateExportLabel), typeof(string), typeof(DocumentToolbar),
            new PropertyMetadata("Lập phiếu xuất"));

    public static readonly DependencyProperty ShowCloseProperty =
        DependencyProperty.Register(nameof(ShowClose), typeof(bool), typeof(DocumentToolbar),
            new PropertyMetadata(true));

    public string SaveLabel         { get => (string)GetValue(SaveLabelProperty);         set => SetValue(SaveLabelProperty, value); }
    public string CreateExportLabel { get => (string)GetValue(CreateExportLabelProperty); set => SetValue(CreateExportLabelProperty, value); }
    public bool   ShowClose         { get => (bool)GetValue(ShowCloseProperty);           set => SetValue(ShowCloseProperty, value); }

    // ─── CloseRequested routed event ────────────────────────────────────────

    public static readonly RoutedEvent CloseRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(CloseRequested), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(DocumentToolbar));

    public event RoutedEventHandler CloseRequested
    {
        add    => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, this));
}
