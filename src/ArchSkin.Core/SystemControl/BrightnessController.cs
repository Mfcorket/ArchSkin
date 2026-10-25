using System.Runtime.InteropServices;

namespace ArchSkin.Core.SystemControl;

/// <summary>
/// Controls monitor brightness.
/// Primary path: DDC/CI via dxva2.dll (works on most external monitors).
/// Fallback: WMI WmiMonitorBrightness (works on many laptop panels).
/// </summary>
public sealed class BrightnessController : IDisposable
{
    private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;

    private IntPtr _physicalMonitor = IntPtr.Zero;
    private uint _min = 0;
    private uint _max = 100;
    private bool _ddcInitialized;
    private bool _ddcAvailable;

    public bool IsAvailable => _ddcAvailable || WmiAvailable();

    public int GetBrightness()
    {
        if (TryInitDdc())
        {
            try
            {
                if (GetMonitorBrightness(_physicalMonitor, out var min, out var current, out var max) == 0 && max > min)
                {
                    return (int)Math.Round((current - min) * 100.0 / (max - min));
                }
            }
            catch
            {
            }
        }

        var wmi = GetWmiBrightness();
        return wmi ?? 80;
    }

    public bool SetBrightness(int percent)
    {
        percent = Math.Clamp(percent, 0, 100);

        if (TryInitDdc())
        {
            try
            {
                if (GetMonitorBrightness(_physicalMonitor, out var min, out _, out var max) == 0 && max > min)
                {
                    var target = (uint)Math.Round(min + (max - min) * (percent / 100.0));
                    if (SetMonitorBrightness(_physicalMonitor, target) == 0) return true;
                }
            }
            catch
            {
            }
        }

        return SetWmiBrightness(percent);
    }

    private bool TryInitDdc()
    {
        if (_ddcInitialized) return _ddcAvailable;
        _ddcInitialized = true;

        try
        {
            var hMonitor = MonitorFromWindow(GetForegroundWindow(), MONITOR_DEFAULTTOPRIMARY);
            if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var count) != 0 || count == 0)
                return false;

            var monitors = new PHYSICAL_MONITOR[count];
            if (GetPhysicalMonitorsFromHMONITOR(hMonitor, count, monitors) != 0)
                return false;

            _physicalMonitor = monitors[0].hPhysicalMonitor;
            if (GetMonitorBrightness(_physicalMonitor, out _min, out _, out _max) != 0)
                return false;

            _ddcAvailable = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int? GetWmiBrightness()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "root\\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
            foreach (var obj in searcher.Get())
            {
                var value = obj["CurrentBrightness"];
                if (value != null) return Convert.ToInt32(value);
            }
        }
        catch
        {
        }

        return null;
    }

    private static bool SetWmiBrightness(int percent)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "root\\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
            foreach (var baseObj in searcher.Get())
            {
                if (baseObj is not System.Management.ManagementObject obj) continue;

                using var args = obj.GetMethodParameters("WmiSetBrightness");
                args["Timeout"] = 1;
                args["Brightness"] = percent;
                using var result = obj.InvokeMethod("WmiSetBrightness", args, null);
                return result != null;
            }
        }
        catch
        {
        }

        return false;
    }

    private static bool WmiAvailable()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "root\\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
            return searcher.Get().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_physicalMonitor != IntPtr.Zero)
        {
            try { DestroyPhysicalMonitor(_physicalMonitor); } catch { }
            _physicalMonitor = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("dxva2.dll")]
    private static extern int GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint count);

    [DllImport("dxva2.dll")]
    private static extern int GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint size, [Out] PHYSICAL_MONITOR[] monitors);

    [DllImport("dxva2.dll")]
    private static extern int GetMonitorBrightness(IntPtr hMonitor, out uint min, out uint current, out uint max);

    [DllImport("dxva2.dll")]
    private static extern int SetMonitorBrightness(IntPtr hMonitor, uint newBrightness);

    [DllImport("dxva2.dll")]
    private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);
}
