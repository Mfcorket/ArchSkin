using System.Runtime.InteropServices;
using ArchSkin.Core.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace ArchSkin.Shell;

public static class WindowHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    public static IntPtr GetHwnd(Window window) => WindowNative.GetWindowHandle(window);

    public static void Move(this Window window, int x, int y)
    {
        var hwnd = GetHwnd(window);
        SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public static void Resize(this Window window, int width, int height)
    {
        var hwnd = GetHwnd(window);
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public static void SetTopmost(this Window window, bool topmost)
    {
        var hwnd = GetHwnd(window);
        var insertAfter = topmost ? new IntPtr(-1) : new IntPtr(-2);
        SetWindowPos(hwnd, insertAfter, 0, 0, 0, 0, SWP_NOACTIVATE | 0x0002 | 0x0001);
    }

    public static void MakeNonResizable(this Window window)
    {
        if (window.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }
    }

    /// <summary>Logical (96-DPI) size of the primary monitor.</summary>
    public static (int Width, int Height) GetScreenSize()
    {
        var monitor = MonitorManager.GetPrimaryMonitor();
        if (monitor == null) return (1920, 1080);

        var scale = GetPrimaryScale();
        return ((int)(monitor.Width / scale), (int)(monitor.Height / scale));
    }

    /// <summary>Logical bounds of the primary monitor, including its origin.</summary>
    public static (int X, int Y, int Width, int Height) GetPrimaryBounds()
    {
        var monitor = MonitorManager.GetPrimaryMonitor();
        if (monitor == null) return (0, 0, 1920, 1080);

        var scale = GetPrimaryScale();
        return (
            (int)(monitor.Left / scale),
            (int)(monitor.Top / scale),
            (int)(monitor.Width / scale),
            (int)(monitor.Height / scale));
    }

    /// <summary>Logical bounds of the monitor containing the given window.</summary>
    public static (int X, int Y, int Width, int Height) GetBoundsForWindow(Window window)
    {
        var hwnd = GetHwnd(window);
        var monitor = MonitorManager.GetMonitorForWindow(hwnd) ?? MonitorManager.GetPrimaryMonitor();
        if (monitor == null) return (0, 0, 1920, 1080);

        var scale = GetScaleForWindow(hwnd);
        return (
            (int)(monitor.Left / scale),
            (int)(monitor.Top / scale),
            (int)(monitor.Width / scale),
            (int)(monitor.Height / scale));
    }

    public static double GetScaleForWindow(IntPtr hwnd)
    {
        try
        {
            var dpi = GetDpiForWindow(hwnd);
            if (dpi == 0) return 1.0;
            return dpi / 96.0;
        }
        catch
        {
            return 1.0;
        }
    }

    private static double GetPrimaryScale()
    {
        try
        {
            var info = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            var scale = info.RawDpiX / 96.0;
            return scale <= 0 ? 1.0 : scale;
        }
        catch
        {
            return 1.0;
        }
    }
}
