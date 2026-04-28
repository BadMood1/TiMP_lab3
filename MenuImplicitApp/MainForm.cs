using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using DataMenuLibrary;

namespace MenuImplicitApp;

public class MainForm : Form
{
    private readonly Dictionary<string, int> _statuses;
    private readonly DataMenu _menu;

    public MainForm(Dictionary<string, int> statuses)
    {
        _statuses = statuses;
        _menu = new DataMenu(FileLocator.FindFile("menu.txt"));

        InitializeUi();
        BuildMenu();
    }

    private void InitializeUi()
    {
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Text = "АИС Отдел кадров (неявное связывание)";
    }

    private void BuildMenu()
    {
        var menuStrip = new MenuStrip();

        foreach (var root in _menu.Roots)
        {
            var item = CreateMenuItem(root);
            if (item != null)
            {
                menuStrip.Items.Add(item);
            }
        }

        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);
    }

    private ToolStripMenuItem? CreateMenuItem(MenuNode node)
    {
        var status = _statuses.TryGetValue(node.Title, out var resolvedStatus) ? resolvedStatus : 0;
        if (status == 2)
        {
            return null;
        }

        var item = new ToolStripMenuItem(node.Title)
        {
            Enabled = status != 1
        };

        foreach (var child in node.Children)
        {
            var childItem = CreateMenuItem(child);
            if (childItem != null)
            {
                item.DropDownItems.Add(childItem);
            }
        }

        if (!string.IsNullOrWhiteSpace(node.MethodName))
        {
            item.Click += (_, _) => InvokeHandler(node.MethodName!);
        }

        return item;
    }

    private void InvokeHandler(string methodName)
    {
        var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            MessageBox.Show($"Метод не найден: {methodName}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        method.Invoke(this, null);
    }

    public void Others() => MessageBox.Show("Разное", "Меню");
    public void Stuff() => MessageBox.Show("Сотрудники", "Меню");
    public void Orders() => MessageBox.Show("Приказы", "Меню");
    public void Docs() => MessageBox.Show("Документы", "Меню");
    public void Departs() => MessageBox.Show("Отделы", "Меню");
    public void Towns() => MessageBox.Show("Города", "Меню");
    public void Posts() => MessageBox.Show("Должности", "Меню");
    public void Window() => MessageBox.Show("Окно", "Меню");
    public void Help() => MessageBox.Show("Справка", "Меню");
    public void Content() => MessageBox.Show("Оглавление", "Меню");
    public void About() => MessageBox.Show("О программе", "Меню");

    // Use FileLocator.FindFile instead of local PathFor helper
}
