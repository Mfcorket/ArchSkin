using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Shell.Views;

public sealed partial class CalendarPopperView : UserControl
{
    public CalendarViewModel ViewModel { get; }

    public event EventHandler? CloseRequested;

    public CalendarPopperView(CalendarViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;
        DaysGrid.ItemsSource = ViewModel.Days;
    }

    private void OnPreviousClick(object sender, RoutedEventArgs e)
    {
        ViewModel.PreviousMonth();
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
        ViewModel.NextMonth();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
