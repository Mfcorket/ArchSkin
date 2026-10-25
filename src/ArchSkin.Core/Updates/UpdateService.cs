using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchSkin.Core.Updates;

public sealed record ReleaseInfo(
    string Version,
    string? Notes,
    string? DownloadUrl,
    DateTimeOffset? PublishedAt);

public sealed record UpdateCheckResult(
    bool UpdateAvailable,
    string CurrentVersion,
    ReleaseInfo? Latest,
    string? Error);

public sealed class UpdateService
{
    public const string CurrentVersion = "1.0.0";

    private static readonly string StatePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArchSkin", "update-state.json");

    private readonly HttpClient _http;

    public UpdateService(HttpClient? http = null)
    {
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", $"ArchSkin/{CurrentVersion}");
        }
    }

    public async Task<UpdateCheckResult> CheckAsync(Uri feedUri, CancellationToken ct = default)
    {
        try
        {
            var json = await _http.GetStringAsync(feedUri, ct);
            var latest = JsonSerializer.Deserialize<ReleaseInfo>(json, JsonOptions);

            if (latest == null || string.IsNullOrWhiteSpace(latest.Version))
            {
                return new UpdateCheckResult(false, CurrentVersion, null, "Invalid release feed");
            }

            var available = CompareVersions(latest.Version, CurrentVersion) > 0;
            SaveLastCheck(available ? latest.Version : null);
            return new UpdateCheckResult(available, CurrentVersion, latest, null);
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(false, CurrentVersion, null, ex.Message);
        }
    }

    public static int CompareVersions(string a, string b)
    {
        var pa = ParseVersion(a);
        var pb = ParseVersion(b);
        var len = Math.Max(pa.Length, pb.Length);

        for (var i = 0; i < len; i++)
        {
            var va = i < pa.Length ? pa[i] : 0;
            var vb = i < pb.Length ? pb[i] : 0;
            if (va != vb) return va.CompareTo(vb);
        }

        return 0;
    }

    private static int[] ParseVersion(string version)
    {
        var core = version.Trim().TrimStart('v', 'V');
        var dash = core.IndexOfAny(new[] { '-', '+' });
        if (dash >= 0) core = core[..dash];

        return core.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => int.TryParse(p, out var n) ? n : 0)
            .ToArray();
    }

    public DateTimeOffset? GetLastCheckTime()
    {
        try
        {
            if (!File.Exists(StatePath)) return null;
            var state = JsonSerializer.Deserialize<UpdateState>(File.ReadAllText(StatePath), JsonOptions);
            return state?.LastCheckUtc;
        }
        catch
        {
            return null;
        }
    }

    public bool IsCheckDue(int frequencyHours)
    {
        var last = GetLastCheckTime();
        if (last == null) return true;
        return DateTimeOffset.UtcNow - last.Value >= TimeSpan.FromHours(Math.Max(1, frequencyHours));
    }

    private void SaveLastCheck(string? latestVersion)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StatePath)!);
            var state = new UpdateState
            {
                LastCheckUtc = DateTimeOffset.UtcNow,
                LatestKnownVersion = latestVersion
            };
            File.WriteAllText(StatePath, JsonSerializer.Serialize(state, JsonOptions));
        }
        catch
        {
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private sealed class UpdateState
    {
        [JsonPropertyName("lastCheckUtc")]
        public DateTimeOffset? LastCheckUtc { get; set; }

        [JsonPropertyName("latestKnownVersion")]
        public string? LatestKnownVersion { get; set; }
    }
}
