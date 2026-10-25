using ArchSkin.Core.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Management;

namespace ArchSkin.Login;

public sealed partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private AppSettings _settings;

    public MainWindow()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();

        this.InitializeComponent();
        this.Title = "ArchSkin Login";
        
        LoadUser();
    }

    private void LoadUser()
    {
        // TODO: Enumerate local users from SAM or WMI
        UserButton.Content = Environment.UserName;
    }

    private void OnUserClick(object sender, RoutedEventArgs e)
    {
        UserSelection.Visibility = Visibility.Collapsed;
        PasswordPanel.Visibility = Visibility.Visible;
        PasswordBox.Focus(FocusState.Programmatic);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        PasswordPanel.Visibility = Visibility.Collapsed;
        UserSelection.Visibility = Visibility.Visible;
        PasswordBox.Password = "";
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private async void OnSignInClick(object sender, RoutedEventArgs e)
    {
        var password = PasswordBox.Password;
        if (string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your password");
            return;
        }

        // Verify password via Windows API
        if (VerifyPassword(Environment.UserName, password))
        {
            // Password correct - close overlay and let LogonUI continue
            Close();
        }
        else
        {
            ShowError("Incorrect password. Try again.");
            PasswordBox.Password = "";
        }
    }

    private bool VerifyPassword(string username, string password)
    {
        try
        {
            // Use Windows LogonUser API
            return LogonUser(username, ".", password, 2, 0, out var token);
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);
}