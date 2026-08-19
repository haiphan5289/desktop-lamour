// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Diagnostics;
using System.IO;
using ClosedXML.Excel;

namespace DesktopLamour.Shared.Helpers;

// Shared OS-interop for sending a generated report file via the user's own Email/Zalo desktop apps.
// Neither integration has an API wired up (no SMTP, no Zalo OA token), so this just stages the
// exported file and hands off to whatever client is installed for the user to attach manually.
public static class ReportSharingHelper
{
    public static string SaveWorkbookToTempFile(XLWorkbook workbook, string fileNamePrefix)
    {
        var fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var path     = Path.Combine(Path.GetTempPath(), fileName);
        workbook.SaveAs(path);
        return path;
    }

    public static void RevealInExplorer(string filePath) =>
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true });

    public static void OpenMailClient(string subject, string body)
    {
        var uri = $"mailto:?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    public static void OpenZaloApp()
    {
        var zaloExe = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "Zalo", "Zalo.exe");

        try
        {
            if (File.Exists(zaloExe))
                Process.Start(new ProcessStartInfo(zaloExe) { UseShellExecute = true });
        }
        catch (Exception)
        {
            // Zalo not installed or failed to launch — the file is already revealed in
            // Explorer, so the user can still attach it manually once Zalo is open.
        }
    }
}
