using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AuthorizationLibrary;
using DataMenuLibrary;

namespace MenuImplicitApp;

public partial class AuthorizationWindow : Window
{
    public AuthorizationWindow()
    {
        InitializeComponent();
    }

    private void OnLoginClicked(object? sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text?.Trim() ?? string.Empty;
        var password = PasswordTextBox.Text?.Trim() ?? string.Empty;

        try
        {
            var usersPath = Path.Combine(AppContext.BaseDirectory, "USERS.txt");
            var menuPath = Path.Combine(AppContext.BaseDirectory, "menu.txt");

            var authService = new AuthorizationService(usersPath);
            var statuses = authService.Authenticate(username, password);
            if (statuses == null)
            {
                ErrorTextBlock.Text = "Неверный логин или пароль.";
                return;
            }

            var menu = new DataMenu(menuPath);
            var mainWindow = new MainWindow(menu, statuses);
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = ex.Message;
        }
    }
}
