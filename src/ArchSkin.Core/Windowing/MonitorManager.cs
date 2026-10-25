using System.Runtime.InteropServices;

namespace ArchSkin.Core.Windowing;

public sealed record MonitorInfo(
    IntPtr Handle,
    int Left,
    int Top,
    int Width,
    int Height,
    bool IsPrimary)
{
    public int Right => Left + Width;
    public int Bottom => Top + Height;
}

/// <summary>
/// Enumerates displays using Win32 monitor APIs so the shell can place
/// the panel and dock on the correct monitor.
/// </summary>
public static class MonitorManager
{
    private const uint MONITORINFOF_PRIMARY = 0x00000001;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    public static IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, _, _, _) =>
        {
            var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hMonitor, ref info))
            {
                var r = info.rcMonitor;
                monitors.Add(new MonitorInfo(
                    hMonitor,
                    r.Left,
                    r.Top,
                    r.Right - r.Left,
                    r.Bottom - r.Top,
                    (info.dwFlags & MONITORINFOF_PRIMARY) != 0));
            }
            return true;
        }, IntPtr.Zero);

        return monitors;
    }

    public static MonitorInfo? GetPrimaryMonitor()
    {
        return GetMonitors().FirstOrDefault(m => m.IsPrimary) ?? GetMonitors().FirstOrDefault();
    }

    public static MonitorInfo? GetMonitorForWindow(IntPtr hwnd)
    {
        var hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        if (hMonitor == IntPtr.Zero) return GetPrimaryMonitor();

        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(hMonitor, ref info)) return GetPrimaryMonitor();

        var r = info.rcMonitor;
        return new MonitorInfo(hMonitor, r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top,
            (info.dwFlags & MONITORINFOF_PRIMARY) != 0);
    }

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, IntPtr rect, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc proc, IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO info);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
}
