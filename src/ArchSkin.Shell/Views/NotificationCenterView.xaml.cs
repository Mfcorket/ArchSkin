using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace ArchSkin.Shell.Views;

public sealed partial class NotificationCenterView : UserControl
{
    public NotificationCenterViewModel ViewModel { get; }

    public event EventHandler? CloseRequested;
    public event EventHandler<string>? ActionClicked;

    public NotificationCenterView(NotificationCenterViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;
        NotificationList.ItemsSource = viewModel.Notifications;
        Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        ViewModel.Clear();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnActionClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string actionId)
        {
            ActionClicked?.Invoke(this, actionId);
        }
    }
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}