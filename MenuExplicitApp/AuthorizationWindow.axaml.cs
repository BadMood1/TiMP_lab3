using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MenuExplicitApp;

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
            var baseDir = AppContext.BaseDirectory;
            var usersPath = Path.Combine(baseDir, "USERS.txt");
            var menuPath = Path.Combine(baseDir, "menu.txt");

            var authDllPath = Path.Combine(baseDir, "AuthorizationLibrary.dll");
            var menuDllPath = Path.Combine(baseDir, "DataMenuLibrary.dll");

            if (!File.Exists(authDllPath))
            {
                ErrorTextBlock.Text = "Не найдена AuthorizationLibrary.dll в папке запуска.";
                return;
            }

            if (!File.Exists(menuDllPath))
            {
                ErrorTextBlock.Text = "Не найдена DataMenuLibrary.dll в папке запуска.";
                return;
            }

            var authAssembly = Assembly.LoadFrom(authDllPath);
            var authType = authAssembly.GetType("AuthorizationLibrary.AuthorizationService", throwOnError: true)!;
            var authInstance = Activator.CreateInstance(authType, new object[] { usersPath })!;

            var authMethod = authType.GetMethod("Authenticate", new[] { typeof(string), typeof(string) });
            if (authMethod == null)
            {
                ErrorTextBlock.Text = "В AuthorizationLibrary не найден метод Authenticate.";
                return;
            }

            var authResult = authMethod.Invoke(authInstance, new object[] { username, password });
            var statuses = authResult as Dictionary<string, int>;
            if (statuses == null)
            {
                ErrorTextBlock.Text = "Неверный логин или пароль.";
                return;
            }

            var menuAssembly = Assembly.LoadFrom(menuDllPath);
            var menuType = menuAssembly.GetType("DataMenuLibrary.DataMenu", throwOnError: true)!;
            var menuInstance = Activator.CreateInstance(menuType, new object[] { menuPath })!;

            var rootsObj = menuType.GetProperty("Roots")?.GetValue(menuInstance);
            if (rootsObj is not System.Collections.IEnumerable enumerableRoots)
            {
                ErrorTextBlock.Text = "Не удалось прочитать Roots из DataMenu.";
                return;
            }

            var roots = new List<object>();
            foreach (var item in enumerableRoots)
            {
                if (item != null)
                {
                    roots.Add(item);
                }
            }

            var mainWindow = new MainWindow(roots, statuses);
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = ex.Message;
        }
    }
}
