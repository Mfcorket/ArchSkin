namespace ArchSkin.Core.Localization;

public static class TestAccess
{
    public static IReadOnlyCollection<string> EnKeys() => Strings.En.Keys.ToList();
    public static IReadOnlyCollection<string> ZhKeys() => Strings.ZhCN.Keys.ToList();
    public static IReadOnlyCollection<string> DeKeys() => Strings.De.Keys.ToList();
}
