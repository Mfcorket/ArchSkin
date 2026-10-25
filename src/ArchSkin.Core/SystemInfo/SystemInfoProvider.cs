using System.Net.NetworkInformation;

namespace ArchSkin.Core.SystemInfo;

public sealed class SystemInfoProvider
{
    public float GetBatteryPercent()
    {
        try
        {
            var status = System.Windows.Forms.SystemInformation.PowerStatus;
            var pct = status.BatteryLifePercent;
            if (pct < 0 || pct > 1) return 1f;
            return pct;
        }
        catch
        {
            return 1f;
        }
    }

    public bool IsOnBattery()
    {
        try
        {
            return System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus
                == System.Windows.Forms.PowerLineStatus.Offline;
        }
        catch
        {
            return false;
        }
    }

    public bool IsNetworkAvailable()
    {
        try
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        catch
        {
            return false;
        }
    }

    public string GetNetworkStatusIcon()
    {
        return IsNetworkAvailable() ? "\uE701" : "\uE704";
    }

    public string GetBatteryStatusIcon()
    {
        var pct = GetBatteryPercent();
        if (pct > 0.9f) return "\uE8BF";
        if (pct > 0.75f) return "\uE8BB";
        if (pct > 0.5f) return "\uE8BA";
        if (pct > 0.25f) return "\uE8B9";
        return "\uE8B8";
    }

    public string GetVolumeIcon() => "\uE767";

    public DateTime Now => DateTime.Now;
}
