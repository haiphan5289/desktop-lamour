// AppButton.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows.Controls;

namespace DesktopLamour.Shared.Controls;

/// <summary>
/// Design system button. Always use instead of raw Button.
/// Apply styles via AppButton.Primary.Large / AppButton.Secondary.Medium / AppButton.Tertiary.Medium etc.
/// </summary>
public class AppButton : Button
{
    static AppButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppButton),
            new System.Windows.FrameworkPropertyMetadata(typeof(AppButton)));
    }
}
