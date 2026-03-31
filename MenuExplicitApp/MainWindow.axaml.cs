using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;

namespace MenuExplicitApp;

public partial class MainWindow : Window
{
    private IEnumerable<object> _roots = Array.Empty<object>();
    private Dictionary<string, int> _statuses = new(StringComparer.Ordinal);
    private DemoMenuActions _actions = new(selected => { });

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(IEnumerable<object> roots, Dictionary<string, int> statuses)
    {
        _roots = roots;
        _statuses = statuses;
        _actions = new DemoMenuActions(selected => ResultTextBlock.Text = selected);

        InitializeComponent();
        BuildMenu();
    }

    private void BuildMenu()
    {
        MenuHost.Items.Clear();

        foreach (var root in _roots)
        {
            MenuHost.Items.Add(CreateMenuItem(root));
        }
    }

    private MenuItem CreateMenuItem(object node)
    {
        var nodeType = node.GetType();
        var title = (string)nodeType.GetProperty("Title")!.GetValue(node)!;
        var methodName = nodeType.GetProperty("MethodName")!.GetValue(node) as string;
        var children = nodeType.GetProperty("Children")!.GetValue(node) as System.Collections.IEnumerable;

        var status = _statuses.TryGetValue(title, out var s) ? s : 0;

        var item = new MenuItem
        {
            Header = title,
            IsVisible = status != 2,
            IsEnabled = status != 1,
        };

        if (children != null)
        {
            foreach (var child in children)
            {
                item.Items.Add(CreateMenuItem(child!));
            }
        }

        if (!string.IsNullOrWhiteSpace(methodName) && status != 2)
        {
            item.Click += (_, _) => InvokeHandler(methodName!, title);
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