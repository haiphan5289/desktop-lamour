// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

public partial class SalesReturnWindow : Window
{
    public SalesReturnViewModel ViewModel { get; }

    private SalesReturnResponseDto? _initialReturn;

    public SalesReturnWindow(SalesReturnViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };
        LinesDataGrid.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnProductCellTextChanged));
    }

    public void Initialize(SalesReturnResponseDto? salesReturn)
        => _initialReturn = salesReturn;

    // Cho phép Trước/Sau/Thêm duyệt ngay trong popup khi mở từ 1 danh sách đã tải sẵn (vd.
    // SalesReturnListViewModel.SalesReturns) — không gọi thì Trước/Sau chỉ là no-op.
    public void SetSiblingContext(IReadOnlyList<SalesReturnResponseDto> siblings, int currentIndex)
        => ViewModel.SetSiblingContext(siblings, currentIndex);

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        try
        {
            await ViewModel.InitializeAsync(_initialReturn);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (ViewModel.IsDirty && DialogResult is null)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đóng? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận đóng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) e.Cancel = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();

    private void OnProductCellTextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.OriginalSource is not TextBox textBox || !textBox.IsKeyboardFocused) return;

        var cell = FindParent<DataGridCell>(textBox);
        if (cell is null) return;

        var typedText = textBox.Text;

        switch (cell.Column?.Header?.ToString())
        {
            case "Mã hàng":
                ViewModel.FilterProductsByCode(typedText);
                if (FindParent<ComboBox>(textBox) is { } combo)
                    combo.IsDropDownOpen = true;
                RestoreTypedTextDeferred(textBox, typedText);
                break;
            case "Tên hàng":
                ViewModel.FilterProductsByName(typedText);
                if (FindParent<ComboBox>(textBox) is { } comboName)
                    comboName.IsDropDownOpen = true;
                RestoreTypedTextDeferred(textBox, typedText);
                break;
        }
    }

    private void LinesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        => ViewModel.ResetProductFilter();

    // Fix lỗi WPF kinh điển: gõ ký tự đầu tiên vào ô Mã hàng/Tên hàng (đang không ở chế độ edit)
    // làm DataGrid vừa phải dựng ComboBox từ CellEditingTemplate vừa forward ký tự đó — ComboBox
    // chưa kịp có focus nên ký tự đầu bị rớt, chỉ ký tự thứ 2 trở đi mới thực sự vào ô. Bắt lại
    // ký tự gốc từ EditingEventArgs rồi tự set vào TextBox bên trong ComboBox sau khi nó Loaded.
    private void LinesDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (e.EditingElement is not ComboBox comboBox) return;
        if (e.EditingEventArgs is not TextCompositionEventArgs textArgs) return;
        var typedChar = textArgs.Text;

        void SeedTypedCharacter(object? s, RoutedEventArgs _)
        {
            comboBox.Loaded -= SeedTypedCharacter;
            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is not TextBox textBox) return;

            textBox.Text = typedChar;
            textBox.CaretIndex = textBox.Text.Length;
            // Bắt buộc focus tường minh vào đúng TextBox vừa seed — DataGridCell.BeginEdit() có thể
            // đã chạy focus-on-edit mặc định TRƯỚC KHI PART_EditableTextBox tồn tại (mới Loaded xong
            // ở đây), nên có thể focus rơi vào ComboBox cha thay vì TextBox con. Gõ tiếng Việt qua bộ
            // gõ hay gửi dồn backspace+retype để ghép dấu — nếu bàn phím không đúng focus, các
            // keystroke ghép dấu tiếp theo bị lạc, vỡ cả từ.
            textBox.Focus();
            Keyboard.Focus(textBox);
            RestoreTypedTextDeferred(textBox, typedChar);
        }

        if (comboBox.IsLoaded)
            SeedTypedCharacter(null, null!);
        else
            comboBox.Loaded += SeedTypedCharacter;
    }

    // ComboBox (IsEditable=True) + việc filter Products (Clear/Add) + IsDropDownOpen=true đều có
    // thể tự đổi lại Text/selection của TextBox con sau khi đã set — không đoán trước được thứ tự
    // event chính xác. Đẩy việc "trả lại đúng text/con trỏ, bỏ chọn" xuống Dispatcher priority
    // ContextIdle (thấp hơn Input/Loaded/Render) để luôn chạy SAU CÙNG, sau khi mọi thao tác nội
    // bộ của WPF (mở dropdown, focus, đồng bộ SelectedItem...) cho keystroke này đã xong.
    private static void RestoreTypedTextDeferred(TextBox textBox, string typedText)
    {
        textBox.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            if (textBox.Text != typedText)
                textBox.Text = typedText;
            if (textBox.CaretIndex != typedText.Length)
                textBox.CaretIndex = typedText.Length;
            if (textBox.SelectionLength != 0)
                textBox.SelectionLength = 0;
        }));
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent is null) return null;
        return parent is T p ? p : FindParent<T>(parent);
    }
}
