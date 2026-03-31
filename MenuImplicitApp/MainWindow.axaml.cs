using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using DataMenuLibrary;

namespace MenuImplicitApp;

public partial class MainWindow : Window
{
    private Dictionary<string, int> _statuses = new(StringComparer.Ordinal);
    private DataMenu? _menu;
    private DemoMenuActions _actions = new(selected => { });

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(DataMenu menu, Dictionary<string, int> statuses)
    {
        _menu = menu;
        _statuses = statuses;
        _actions = new DemoMenuActions(selected => ResultTextBlock.Text = selected);

        InitializeComponent();
        BuildMenu();
    }

    private void BuildMenu()
    {
        if (_menu == null)
        {
            return;
        }

        MenuHost.Items.Clear();

        foreach (var root in _menu.Roots)
        {
            MenuHost.Items.Add(CreateMenuItem(root));
        }
    }

    private MenuItem CreateMenuItem(MenuNode node)
    {
        var status = _statuses.TryGetValue(node.Title, out var s) ? s : 0;

        var item = new MenuItem
        {
            Header = node.Title,
            IsVisible = status != 2,
            IsEnabled = status != 1,
        };

        foreach (var child in node.Children)
        {
            item.Items.Add(CreateMenuItem(child));
        }

        if (!string.IsNullOrWhiteSpace(node.MethodName) && status != 2)
        {
            item.Click += (_, _) => InvokeHandler(node.MethodName!, node.Title);
        }

        return item;
    }

    private void InvokeHandler(string methodName, string selectedTitle)
    {
        var method = _actions.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            ResultTextBlock.Text = $"Метод не найден: {methodName}";
            return;
        }

        var parameters = method.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
        {
            method.Invoke(_actions, new object[] { selectedTitle });
            return;
        }

        if (parameters.Length == 0)
        {
            method.Invoke(_actions, null);
            return;
        }

        ResultTextBlock.Text = "Неверная сигнатура обработчика.";
    }

    private sealed class DemoMenuActions
    {
        private readonly Action<string> _setSelected;

        public DemoMenuActions(Action<string> setSelected)
        {
            _setSelected = setSelected;
        }

        public void Others(string title) => _setSelected(title);
        public void Stuff(string title) => _setSelected(title);
        public void Orders(string title) => _setSelected(title);
        public void Docs(string title) => _setSelected(title);
        public void Window(string title) => _setSelected(title);
        public void Help(string title) => _setSelected(title);
        public void Content(string title) => _setSelected(title);
        public void About(string title) => _setSelected(title);
        public void Departs(string title) => _setSelected(title);
        public void Towns(string title) => _setSelected(title);
        public void Posts(string title) => _setSelected(title);
    }
}