using System.Runtime.InteropServices;

namespace ArchSkin.Core.SystemControl;

/// <summary>
/// Controls the default audio endpoint volume via the Windows Core Audio API.
/// Uses hand-written COM interop so no extra package is required.
/// </summary>
public sealed class AudioVolumeController : IDisposable
{
    private static readonly Guid CLSID_MMDeviceEnumerator = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static readonly Guid IID_IMMDeviceEnumerator = new("A95664D2-9614-4F35-A746-DE8DB63617E6");
    private static readonly Guid IID_IAudioEndpointVolume = new("5CDF2C82-841E-4546-9722-0CF74078229A");

    private const int eRender = 0;
    private const int eMultimedia = 1;

    private IAudioEndpointVolume? _volume;

    public bool IsAvailable => _volume != null;

    public bool TryInitialize()
    {
        if (_volume != null) return true;

        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;
        try
        {
            var type = Type.GetTypeFromCLSID(CLSID_MMDeviceEnumerator);
            if (type == null) return false;

            enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(type)!;

            var hr = enumerator.GetDefaultAudioEndpoint(eRender, eMultimedia, out device);
            if (hr != 0 || device == null) return false;

            var iid = IID_IAudioEndpointVolume;
            hr = device.Activate(ref iid, 1 /*CLSCTX_INPROC_SERVER*/, IntPtr.Zero, out var obj);
            if (hr != 0 || obj == null) return false;

            _volume = (IAudioEndpointVolume)obj;
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (device != null) Marshal.ReleaseComObject(device);
            if (enumerator != null) Marshal.ReleaseComObject(enumerator);
        }
    }

    /// <summary>Gets the master volume as a percentage (0-100).</summary>
    public int GetVolume()
    {
        if (!TryInitialize() || _volume == null) return 50;
        try
        {
            var hr = _volume.GetMasterVolumeLevelScalar(out var scalar);
            if (hr != 0) return 50;
            return (int)Math.Round(scalar * 100);
        }
        catch
        {
            return 50;
        }
    }

    /// <summary>Sets the master volume from a percentage (0-100).</summary>
    public bool SetVolume(int percent)
    {
        if (!TryInitialize() || _volume == null) return false;
        try
        {
            var scalar = Math.Clamp(percent, 0, 100) / 100f;
            return _volume.SetMasterVolumeLevelScalar(scalar, Guid.Empty) == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool IsMuted()
    {
        if (!TryInitialize() || _volume == null) return false;
        try
        {
            return _volume.GetMute(out var mute) == 0 && mute;
        }
        catch
        {
            return false;
        }
    }

    public bool SetMute(bool muted)
    {
        if (!TryInitialize() || _volume == null) return false;
        try
        {
            return _volume.SetMute(muted, Guid.Empty) == 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_volume != null)
        {
            Marshal.ReleaseComObject(_volume);
            _volume = null;
        }
    }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices);
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);
        int GetDevice(string id, out IMMDevice device);
        int RegisterEndpointNotificationCallback(IntPtr client);
        int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
        int OpenPropertyStore(int access, out IntPtr properties);
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        int GetState(out int state);
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IntPtr notify);
        int UnregisterControlChangeNotify(IntPtr notify);
        int GetChannelCount(out uint count);
        int SetMasterVolumeLevel(float level, Guid eventContext);
        int SetMasterVolumeLevelScalar(float level, Guid eventContext);
        int GetMasterVolumeLevel(out float level);
        int GetMasterVolumeLevelScalar(out float level);
        int SetChannelVolumeLevel(uint channel, float level, Guid eventContext);
        int SetChannelVolumeLevelScalar(uint channel, float level, Guid eventContext);
        int GetChannelVolumeLevel(uint channel, out float level);
        int GetChannelVolumeLevelScalar(uint channel, out float level);
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, Guid eventContext);
        int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
        int GetVolumeStepInfo(out uint step, out uint stepCount);
        int VolumeStepUp(Guid eventContext);
        int VolumeStepDown(Guid eventContext);
        int QueryHardwareSupport(out uint hardwareSupportMask);
        int GetVolumeRange(out float min, out float max, out float increment);
    }
}
