using System.Runtime.InteropServices;

namespace ArchSkin.Core.HotkeyManager;

public class HotkeyManager : IDisposable
{
    private readonly Dictionary<string, HotkeyRegistration> _registrations = new();
    private IntPtr _hwnd;
    private uint _hotkeyId;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    public void SetWindowHandle(IntPtr hwnd) => _hwnd = hwnd;

    public bool Register(string name, HotkeyCombo combo, Action handler)
    {
        if (_hwnd == IntPtr.Zero) return false;

        var id = (int)++_hotkeyId;
        var modifiers = GetModifiers(combo);

        if (!RegisterHotKey(_hwnd, id, modifiers, (uint)combo.Key))
            return false;

        _registrations[name] = new HotkeyRegistration(id, combo, handler);
        return true;
    }

    public bool Unregister(string name)
    {
        if (!_registrations.TryGetValue(name, out var reg)) return false;

        var result = UnregisterHotKey(_hwnd, reg.Id);
        _registrations.Remove(name);
        return result;
    }

    public bool IsRegistered(string name) => _registrations.ContainsKey(name);

    public bool HandleHotkey(int id)
    {
        foreach (var reg in _registrations.Values)
        {
            if (reg.Id == id)
            {
                reg.Handler();
                return true;
            }
        }
        return false;
    }

    private static uint GetModifiers(HotkeyCombo combo)
    {
        uint mods = 0;
        if (combo.Ctrl) mods |= MOD_CONTROL;
        if (combo.Alt) mods |= MOD_ALT;
        if (combo.Shift) mods |= MOD_SHIFT;
        if (combo.Win) mods |= MOD_WIN;
        return mods;
    }

    public void Dispose()
    {
        foreach (var reg in _registrations.Values)
        {
            UnregisterHotKey(_hwnd, reg.Id);
        }
        _registrations.Clear();
    }

    private sealed record HotkeyRegistration(int Id, HotkeyCombo Combo, Action Handler);
}

public sealed record HotkeyCombo(bool Ctrl, bool Alt, bool Shift, bool Win, Key Key);

public enum Key
{
    None = 0,
    Space = 0x20,
    Tab = 0x09,
    Enter = 0x0D,
    Escape = 0x1B,
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,
}
