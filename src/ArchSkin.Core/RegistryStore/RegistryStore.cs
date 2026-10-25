using Microsoft.Win32;

namespace ArchSkin.Core.RegistryStore;

public static class RegistryStore
{
    private const string BaseKey = @"Software\ArchSkin";

    public static T? GetValue<T>(string name, T? defaultValue = default)
    {
        using var key = Registry.CurrentUser.OpenSubKey(BaseKey);
        if (key == null) return defaultValue;
        var value = key.GetValue(name);
        if (value == null) return defaultValue;
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public static void SetValue<T>(string name, T value)
    {
        if (value is null) return;
        using var key = Registry.CurrentUser.CreateSubKey(BaseKey);
        key?.SetValue(name, value);
    }

    public static void DeleteValue(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(BaseKey, writable: true);
        key?.DeleteValue(name, throwOnMissingValue: false);
    }

    public static bool ValueExists(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(BaseKey);
        return key?.GetValue(name) != null;
    }
}
