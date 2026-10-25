namespace ArchSkin.Core.Localization;

public enum AppLanguage
{
    English,
    ChineseSimplified,
    German
}

public static class LocalizationService
{
    private static AppLanguage _current = AppLanguage.English;

    public static AppLanguage Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            LanguageChanged?.Invoke(null, value);
        }
    }

    public static event EventHandler<AppLanguage>? LanguageChanged;

    public static string Get(string key)
    {
        var table = _current switch
        {
            AppLanguage.ChineseSimplified => Strings.ZhCN,
            AppLanguage.German => Strings.De,
            _ => Strings.En
        };

        if (table.TryGetValue(key, out var value)) return value;
        return Strings.En.TryGetValue(key, out var fallback) ? fallback : key;
    }

    public static AppLanguage Parse(string? code)
    {
        return code?.ToLowerInvariant() switch
        {
            "zh" or "zh-cn" or "zh-hans" => AppLanguage.ChineseSimplified,
            "de" or "de-de" => AppLanguage.German,
            _ => AppLanguage.English
        };
    }

    public static string ToCode(AppLanguage language) => language switch
    {
        AppLanguage.ChineseSimplified => "zh-CN",
        AppLanguage.German => "de-DE",
        _ => "en-US"
    };

    public static AppLanguage DetectFromSystem()
    {
        return Parse(System.Globalization.CultureInfo.CurrentUICulture.Name);
    }
}

internal static class Strings
{
    public static readonly Dictionary<string, string> En = new()
    {
        ["app.name"] = "ArchSkin",
        ["app.tagline"] = "Give your Windows an Arch-grade look.",
        ["panel.activities"] = "Activities",
        ["panel.apps"] = "Apps",
        ["panel.quickSettings"] = "Quick Settings",
        ["panel.brightness"] = "Brightness",
        ["panel.volume"] = "Volume",
        ["panel.network"] = "Network",
        ["panel.battery"] = "Battery",
        ["panel.performance"] = "Performance",
        ["panel.balanced"] = "Balanced",
        ["panel.settings"] = "ArchSkin Settings",
        ["launcher.search"] = "Search applications...",
        ["launcher.close"] = "Close",
        ["overview.title"] = "Activities",
        ["overview.close"] = "Close Overview",
        ["notifications.title"] = "Notifications",
        ["notifications.clearAll"] = "Clear all",
        ["settings.theme"] = "Theme",
        ["settings.panel"] = "Panel",
        ["settings.dock"] = "Dock",
        ["settings.overview"] = "Overview",
        ["settings.wallpaper"] = "Wallpaper",
        ["settings.updates"] = "Updates",
        ["settings.about"] = "About",
        ["settings.applyTheme"] = "Apply Theme",
        ["settings.installTheme"] = "Install Theme",
        ["settings.verifyLicenses"] = "Verify Licenses",
        ["updates.autoUpdate"] = "Check for updates automatically",
        ["updates.frequency"] = "Check frequency (hours)",
        ["updates.checkNow"] = "Check for Updates Now",
        ["updates.upToDate"] = "You are running the latest version of ArchSkin.",
        ["updates.available"] = "A new version is available: {0}",
        ["updates.checking"] = "Checking for updates...",
        ["updates.failed"] = "Update check failed: {0}",
        ["crash.enable"] = "Send anonymous crash reports",
        ["crash.viewReports"] = "View crash reports",
        ["crash.clearReports"] = "Clear crash reports",
        ["about.version"] = "Version",
        ["about.license"] = "License: MIT",
        ["about.disclaimer"] = "ArchSkin is not affiliated with, endorsed by, or sponsored by Arch Linux.",
        ["common.ok"] = "OK",
        ["common.cancel"] = "Cancel",
        ["common.save"] = "Save",
        ["common.reset"] = "Reset to Default"
    };

    public static readonly Dictionary<string, string> ZhCN = new()
    {
        ["app.name"] = "ArchSkin",
        ["app.tagline"] = "让你的 Windows 拥有 Arch 级外观。",
        ["panel.activities"] = "活动",
        ["panel.apps"] = "应用",
        ["panel.quickSettings"] = "快捷设置",
        ["panel.brightness"] = "亮度",
        ["panel.volume"] = "音量",
        ["panel.network"] = "网络",
        ["panel.battery"] = "电池",
        ["panel.performance"] = "性能",
        ["panel.balanced"] = "均衡",
        ["panel.settings"] = "ArchSkin 设置",
        ["launcher.search"] = "搜索应用程序...",
        ["launcher.close"] = "关闭",
        ["overview.title"] = "活动",
        ["overview.close"] = "关闭概览",
        ["notifications.title"] = "通知",
        ["notifications.clearAll"] = "全部清除",
        ["settings.theme"] = "主题",
        ["settings.panel"] = "面板",
        ["settings.dock"] = "程序坞",
        ["settings.overview"] = "概览",
        ["settings.wallpaper"] = "壁纸",
        ["settings.updates"] = "更新",
        ["settings.about"] = "关于",
        ["settings.applyTheme"] = "应用主题",
        ["settings.installTheme"] = "安装主题",
        ["settings.verifyLicenses"] = "验证许可",
        ["updates.autoUpdate"] = "自动检查更新",
        ["updates.frequency"] = "检查频率（小时）",
        ["updates.checkNow"] = "立即检查更新",
        ["updates.upToDate"] = "您正在使用最新版本的 ArchSkin。",
        ["updates.available"] = "有新版本可用：{0}",
        ["updates.checking"] = "正在检查更新...",
        ["updates.failed"] = "更新检查失败：{0}",
        ["crash.enable"] = "发送匿名崩溃报告",
        ["crash.viewReports"] = "查看崩溃报告",
        ["crash.clearReports"] = "清除崩溃报告",
        ["about.version"] = "版本",
        ["about.license"] = "许可证：MIT",
        ["about.disclaimer"] = "ArchSkin 与 Arch Linux 无关联，也未获得其认可或赞助。",
        ["common.ok"] = "确定",
        ["common.cancel"] = "取消",
        ["common.save"] = "保存",
        ["common.reset"] = "恢复默认"
    };

    public static readonly Dictionary<string, string> De = new()
    {
        ["app.name"] = "ArchSkin",
        ["app.tagline"] = "Verleihen Sie Ihrem Windows einen Arch-Look.",
        ["panel.activities"] = "Aktivitäten",
        ["panel.apps"] = "Apps",
        ["panel.quickSettings"] = "Schnelleinstellungen",
        ["panel.brightness"] = "Helligkeit",
        ["panel.volume"] = "Lautstärke",
        ["panel.network"] = "Netzwerk",
        ["panel.battery"] = "Akku",
        ["panel.performance"] = "Leistung",
        ["panel.balanced"] = "Ausgeglichen",
        ["panel.settings"] = "ArchSkin-Einstellungen",
        ["launcher.search"] = "Anwendungen suchen...",
        ["launcher.close"] = "Schließen",
        ["overview.title"] = "Aktivitäten",
        ["overview.close"] = "Übersicht schließen",
        ["notifications.title"] = "Benachrichtigungen",
        ["notifications.clearAll"] = "Alle löschen",
        ["settings.theme"] = "Design",
        ["settings.panel"] = "Leiste",
        ["settings.dock"] = "Dock",
        ["settings.overview"] = "Übersicht",
        ["settings.wallpaper"] = "Hintergrund",
        ["settings.updates"] = "Updates",
        ["settings.about"] = "Info",
        ["settings.applyTheme"] = "Design anwenden",
        ["settings.installTheme"] = "Design installieren",
        ["settings.verifyLicenses"] = "Lizenzen prüfen",
        ["updates.autoUpdate"] = "Automatisch nach Updates suchen",
        ["updates.frequency"] = "Prüfintervall (Stunden)",
        ["updates.checkNow"] = "Jetzt nach Updates suchen",
        ["updates.upToDate"] = "Sie verwenden die neueste Version von ArchSkin.",
        ["updates.available"] = "Eine neue Version ist verfügbar: {0}",
        ["updates.checking"] = "Suche nach Updates...",
        ["updates.failed"] = "Updateprüfung fehlgeschlagen: {0}",
        ["crash.enable"] = "Anonyme Absturzberichte senden",
        ["crash.viewReports"] = "Absturzberichte anzeigen",
        ["crash.clearReports"] = "Absturzberichte löschen",
        ["about.version"] = "Version",
        ["about.license"] = "Lizenz: MIT",
        ["about.disclaimer"] = "ArchSkin steht in keiner Verbindung zu Arch Linux und wird nicht von Arch Linux unterstützt.",
        ["common.ok"] = "OK",
        ["common.cancel"] = "Abbrechen",
        ["common.save"] = "Speichern",
        ["common.reset"] = "Zurücksetzen"
    };
}
