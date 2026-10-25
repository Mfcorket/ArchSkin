using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;

namespace ArchSkin.Shell;

public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArchSkin", "logs", "shell-crash.log");

    private WindowManager? _windowManager;

    public ShellViewModel? ShellViewModel { get; private set; }

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash("AppDomain", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) => LogCrash("Task", e.Exception);
        this.UnhandledException += (_, e) => LogCrash("Xaml", e.Exception);

        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            ShellViewModel = new ShellViewModel();
            _windowManager = new WindowManager(ShellViewModel);
            _windowManager.Start();

            var toastListener = new ToastListener(ShellViewModel.Notifications);
            toastListener.Start();
        }
        catch (Exception ex)
        {
            LogCrash("OnLaunched", ex);
            throw;
        }
    }

    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            var text = $"[{DateTime.Now:O}] {source}: {ex}\n\n";
            File.AppendAllText(LogPath, text);
        }
        catch
        {
        }
    }
}
