using ArchSkin.Core.AppLauncher;
using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Shell.Views;

public sealed partial class LauncherView : UserControl
{
    public LauncherViewModel ViewModel { get; }

    public event EventHandler? CloseRequested;

    public LauncherView(LauncherViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;
        AppsGrid.ItemsSource = viewModel.Apps;
    }

    private void OnAppClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AppEntry app)
        {
            ViewModel.Launch(app);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
