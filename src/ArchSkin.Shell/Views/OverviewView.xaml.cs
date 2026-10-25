using ArchSkin.Core.ProcessMonitor;
using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ArchSkin.Shell.Views;

public sealed partial class OverviewView : UserControl
{
    public OverviewViewModel ViewModel { get; }

    public event EventHandler? CloseRequested;

    public OverviewView(OverviewViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;

        WorkspaceList.ItemsSource = viewModel.Workspaces;
        WindowsList.ItemsSource = viewModel.Windows;

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateWorkspaceHighlight();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverviewViewModel.ActiveWorkspace))
        {
            UpdateWorkspaceHighlight();
        }
    }

    private void UpdateWorkspaceHighlight()
    {
        for (var i = 0; i < WorkspaceList.Items.Count; i++)
        {
            if (WorkspaceList.ContainerFromIndex(i) is ListViewItem item)
            {
                var border = FindVisualChild<Border>(item);
                if (border != null)
                {
                    border.Background = i == ViewModel.ActiveWorkspace
                        ? new SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue)
                        : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 66, 82));
                }
            }
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private void OnWorkspaceClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is WorkspaceInfo workspace)
        {
            ViewModel.SwitchWorkspace(workspace.Index - 1);
        }
    }

    private void OnWindowClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RunningApp app)
        {
            ViewModel.FocusWindow(app);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}