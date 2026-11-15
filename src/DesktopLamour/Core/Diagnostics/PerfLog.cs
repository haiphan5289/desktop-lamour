// Copyright © 2026 DesktopLamour. All rights reserved.
using System.IO;

namespace DesktopLamour.Core.Diagnostics;

// 2026-11-15: ghi mốc thời gian vào file để tìm bước chậm khi người dùng báo "app đơ/lắc" (không tái hiện
// được trên máy dev, chỉ chạy qua UltraViewer ở máy khách). File: %LocalAppData%\DesktopLamour\perf.log
// (đọc bằng Notepad). Không bao giờ ném lỗi — log hỏng không được làm hỏng thao tác của người dùng.
public static class PerfLog
{
    private static readonly object Gate = new();
    private const long MaxBytes = 1_000_000; // quá ~1MB thì ghi đè từ đầu, khỏi phình mãi

    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopLamour", "perf.log");

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var info = new FileInfo(FilePath);
                if (info.Exists && info.Length > MaxBytes) info.Delete();
                File.AppendAllText(FilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // cố ý nuốt
        }
    }
}
