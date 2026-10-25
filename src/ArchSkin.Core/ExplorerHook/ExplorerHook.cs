using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ArchSkin.Core.ExplorerHook;

public static class ExplorerHook
{
    private const string AdvancedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string ShowDesktopIconsValue = "ShowDesktopIcons";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam,
        uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SHChangeNotify(
        uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

    public static bool IsDesktopIconsVisible
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(AdvancedKey);
            if (key == null) return true;
            var value = key.GetValue(ShowDesktopIconsValue);
            return value is int i && i != 0;
        }
    }

    public static void SetDesktopIconsVisible(bool visible)
    {
        using var key = Registry.CurrentUser.CreateSubKey(AdvancedKey);
        if (key == null) return;
        key.SetValue(ShowDesktopIconsValue, visible ? 1 : 0, RegistryValueKind.DWord);

        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, 5000, out _);
    }

    public static void HideDesktopIcons() => SetDesktopIconsVisible(false);

    public static void ShowDesktopIcons() => SetDesktopIconsVisible(true);
}
