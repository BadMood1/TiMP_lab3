using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MenuExplicitApp;

public class MainForm : Form
{
    private readonly Dictionary<string, int> _statuses;

    public MainForm(Dictionary<string, int> statuses)
    {
        _statuses = statuses;

        InitializeUi();
        BuildMenu();
    }

    private void InitializeUi()
    {
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Text = "АИС Отдел кадров (явное связывание)";
    }

    private void BuildMenu()
    {
        var dllPath = Path.Combine(AppContext.BaseDirectory, "DataMenuLibrary.dll");
        if (!File.Exists(dllPath))
        {
            MessageBox.Show($"Не найдена DataMenuLibrary.dll: {dllPath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var assembly = Assembly.LoadFrom(dllPath);
        var menuType = assembly.GetType("DataMenuLibrary.DataMenu");
        if (menuType == null)
        {
            MessageBox.Show("Класс DataMenuLibrary.DataMenu не найден в DataMenuLibrary.dll", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var menuInstance = Activator.CreateInstance(menuType, new object[] { Path.Combine(AppContext.BaseDirectory, "menu.txt") });
        var roots = menuType.GetProperty("Roots")?.GetValue(menuInstance) as IEnumerable;
        if (roots == null)
        {
            MessageBox.Show("Не удалось прочитать свойство Roots из DataMenuLibrary.DataMenu", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var menuStrip = new MenuStrip();
        foreach (var root in roots)
        {
            if (root == null)
            {
                continue;
            }

            var item = CreateMenuItem(root);
            if (item != null)
            {
                menuStrip.Items.Add(item);
            }
        }

        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);
    }

    private ToolStripMenuItem? CreateMenuItem(object node)
    {
        var nodeType = node.GetType();
        var title = nodeType.GetProperty("Title")?.GetValue(node) as string;
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var status = _statuses.TryGetValue(title, out var resolvedStatus) ? resolvedStatus : 0;
        if (status == 2)
        {
            return null;
        }

        var item = new ToolStripMenuItem(title)
        {
            Enabled = status != 1
        };

        if (nodeType.GetProperty("Children")?.GetValue(node) is IEnumerable children)
        {
            foreach (var child in children)
            {
                if (child == null)
                {
                    continue;
                }

                var childItem = CreateMenuItem(child);
                if (childItem != null)
                {
                    item.DropDownItems.Add(childItem);
                }
            }
        }

        if (nodeType.GetProperty("MethodName")?.GetValue(node) is string methodName && !string.IsNullOrWhiteSpace(methodName))
        {
            item.Click += (_, _) => InvokeHandler(methodName);
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
}
