using System.Text;

namespace ArchSkin.Core.AppLauncher;

public static class ShortcutFileParser
{
    public static bool TryParse(string lnkPath, out ShortcutInfo info)
    {
        info = new ShortcutInfo(string.Empty, null);
        try
        {
            var bytes = File.ReadAllBytes(lnkPath);
            if (bytes.Length < 0x4C) return false;

            var linkFlags = BitConverter.ToUInt32(bytes, 0x14);
            var offset = 0x4C;

            if ((linkFlags & 0x01) != 0)
            {
                if (offset + 2 > bytes.Length) return false;
                var idListSize = BitConverter.ToUInt16(bytes, offset);
                offset += 2 + idListSize;
            }

            if ((linkFlags & 0x02) != 0)
            {
                var start = offset;
                if (start + 0x14 > bytes.Length) return false;

                var headerSize = BitConverter.ToUInt32(bytes, start);
                var infoFlags = BitConverter.ToUInt32(bytes, start + 4);
                var volumeIdOffset = BitConverter.ToUInt32(bytes, start + 8);
                var localBasePathOffset = BitConverter.ToUInt32(bytes, start + 12);
                var commonNetworkOffset = BitConverter.ToUInt32(bytes, start + 16);
                var commonPathSuffixOffset = BitConverter.ToUInt32(bytes, start + 20);

                var localBasePathUnicodeOffset = 0u;
                var commonPathSuffixUnicodeOffset = 0u;
                if (headerSize >= 0x24)
                {
                    if (start + 0x24 > bytes.Length) return false;
                    localBasePathUnicodeOffset = BitConverter.ToUInt32(bytes, start + 24);
                    commonPathSuffixUnicodeOffset = BitConverter.ToUInt32(bytes, start + 28);
                }

                string? localBasePath = null;
                string? commonPathSuffix = null;

                if (localBasePathUnicodeOffset != 0)
                {
                    localBasePath = ReadUtf16(bytes, start + (int)localBasePathUnicodeOffset);
                }
                else if (localBasePathOffset != 0 && (infoFlags & 0x01) != 0)
                {
                    localBasePath = ReadAnsi(bytes, start + (int)localBasePathOffset);
                }

                if (commonPathSuffixUnicodeOffset != 0)
                {
                    commonPathSuffix = ReadUtf16(bytes, start + (int)commonPathSuffixUnicodeOffset);
                }
                else if (commonPathSuffixOffset != 0)
                {
                    commonPathSuffix = ReadAnsi(bytes, start + (int)commonPathSuffixOffset);
                }

                var target = $"{localBasePath}{commonPathSuffix}";
                if (!string.IsNullOrWhiteSpace(target) && File.Exists(target))
                {
                    info = new ShortcutInfo(target, target);
                    return true;
                }

                if (localBasePath != null && File.Exists(localBasePath))
                {
                    info = new ShortcutInfo(localBasePath, localBasePath);
                    return true;
                }
            }

            var fallbackTarget = ScanForPath(bytes);
            if (fallbackTarget != null && File.Exists(fallbackTarget))
            {
                info = new ShortcutInfo(fallbackTarget, fallbackTarget);
                return true;
            }

            var iconIndex = BitConverter.ToInt32(bytes, 0x38);
            var iconLocation = ScanForIconLocation(bytes);
            if (iconLocation != null && File.Exists(iconLocation))
            {
                info = new ShortcutInfo(iconLocation, iconLocation);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string? ReadAnsi(byte[] bytes, int offset)
    {
        if (offset < 0 || offset >= bytes.Length) return null;
        var end = offset;
        while (end < bytes.Length && bytes[end] != 0) end++;
        if (end == offset) return null;
        return Encoding.Default.GetString(bytes, offset, end - offset);
    }

    private static string? ReadUtf16(byte[] bytes, int offset)
    {
        if (offset < 0 || offset >= bytes.Length) return null;
        var end = offset;
        while (end + 1 < bytes.Length && !(bytes[end] == 0 && bytes[end + 1] == 0)) end += 2;
        var length = end - offset;
        if (length <= 0) return null;
        return Encoding.Unicode.GetString(bytes, offset, length);
    }

    private static string? ScanForPath(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if (b >= 32 && b < 127)
            {
                sb.Append((char)b);
            }
            else if (sb.Length > 0)
            {
                var candidate = sb.ToString();
                if (candidate.Length > 4 && candidate.Contains(':'))
                {
                    var candidateTrimmed = candidate.Split(' ')[0];
                    if (File.Exists(candidateTrimmed)) return candidateTrimmed;
                }
                sb.Clear();
            }
        }

        if (sb.Length > 4 && sb.ToString().Contains(':'))
        {
            var candidate = sb.ToString().Split(' ')[0];
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static string? ScanForIconLocation(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if (b >= 32 && b < 127)
            {
                sb.Append((char)b);
            }
            else if (sb.Length > 0)
            {
                var candidate = sb.ToString();
                if (candidate.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                    candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var trimmed = candidate.Split(',')[0];
                    if (File.Exists(trimmed)) return trimmed;
                }
                sb.Clear();
            }
        }
        return null;
    }
}

public sealed record ShortcutInfo(string TargetPath, string? IconPath);
