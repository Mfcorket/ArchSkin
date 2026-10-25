namespace ArchSkin.Core.SystemControl;

/// <summary>
/// Aggregate facade over the individual system control providers.
/// </summary>
public sealed class SystemControl : IDisposable
{
    private readonly AudioVolumeController _audio = new();
    private readonly BrightnessController _brightness = new();

    public AudioVolumeController Audio => _audio;
    public BrightnessController Brightness => _brightness;

    public int GetBrightness() => _brightness.GetBrightness();
    public bool SetBrightness(int percent) => _brightness.SetBrightness(percent);

    public int GetVolume() => _audio.GetVolume();
    public bool SetVolume(int percent) => _audio.SetVolume(percent);

    public bool IsMuted() => _audio.IsMuted();
    public bool SetMute(bool muted) => _audio.SetMute(muted);

    public bool SetNetworkEnabled(bool enabled)
    {
        // Network adapter toggling requires elevation; intentionally a no-op.
        return true;
    }

    public void Dispose()
    {
        _audio.Dispose();
        _brightness.Dispose();
    }
}
