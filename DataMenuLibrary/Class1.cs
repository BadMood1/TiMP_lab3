using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataMenuLibrary;

/// Узел меню в иерархии.
public class MenuNode
{
    public MenuNode(string title, string? methodName)
    {
        Title = title;
        MethodName = methodName;
        Children = new List<MenuNode>();
    }

    public string Title { get; }

    public string? MethodName { get; }

    public List<MenuNode> Children { get; }
}

/// Меню, структура которого полностью задаётся внешним текстовым файлом (программа управляется данными).
/// Меню хранится как множество деревьев (корни имеют уровень 0).
public sealed class DataMenu
{
    private readonly List<MenuNode> _roots;

    public DataMenu(string menuFileName = "menu.txt")
    {
        if (string.IsNullOrWhiteSpace(menuFileName))
        {
            throw new ArgumentException("Имя файла меню не задано.", nameof(menuFileName));
        }

        var menuFilePath = menuFileName;
        if (!Path.IsPathRooted(menuFileName))
        {
            menuFilePath = Path.Combine(AppContext.BaseDirectory, menuFileName);
        }

        if (!File.Exists(menuFilePath))
        {
            throw new FileNotFoundException($"Файл меню не найден: {menuFilePath}", menuFilePath);
        }

        _roots = ParseMenu(menuFilePath);
    }

    public IReadOnlyList<MenuNode> Roots => _roots;

    /// Разбор текстового файла меню в иерархию узлов.
    private static List<MenuNode> ParseMenu(string menuFilePath)
    {
        var roots = new List<MenuNode>();
        var stack = new Stack<(int Level, MenuNode Node)>();

        // Последовательно читаем и разбираем каждую строку файла меню.
        foreach (var rawLine in File.ReadAllLines(menuFilePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            // Поддержка комментариев (на случай если преподаватель добавит).
            if (line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith(";", StringComparison.Ordinal))
            {
                continue;
            }

            // Формат, совместимый с примером из методички:
            // level + Title + (опционально статус) + (опционально имя метода).
            // Примеры из методички (после извлечения текста из PDF):
            // 0 Разное 0 Others
            // 0 Справочники 0           (контейнер, method отсутствует)
            // 1 О программе 0 About     (Title может содержать пробелы)
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(parts[0], out var level))
            {
                continue;
            }

            string title;
            string? methodName = null;

            if (parts.Length == 2)
            {
                title = parts[1];
            }
            else
            {
                // Последний токен либо статус (число), либо имя метода.
                var last = parts[^1];
                var penultimate = parts.Length >= 2 ? parts[^2] : null;
                var lastIsNumeric = int.TryParse(last, out _);

                if (lastIsNumeric)
                {
                    // Контейнер: method не указан, последний токен числовой (например, 0).
                    title = string.Join(" ", parts.Skip(1).Take(parts.Length - 2));
                    methodName = null;
                }
                else
                {
                    methodName = last;

                    // Если перед method стоит числовой токен (например, статус 0 в примере из методички) — пропускаем его,
                    // чтобы в заголовок пункта не попадал служебный статус.
                    if (penultimate != null && int.TryParse(penultimate, out _))
                    {
                        title = string.Join(" ", parts.Skip(1).Take(parts.Length - 3));
                    }
                    else
                    {
                        title = string.Join(" ", parts.Skip(1).Take(parts.Length - 2));
                    }
                }
            }

            var node = new MenuNode(title, methodName);

            // Стек хранит путь от корня до текущего узла.
            // Если уровень уменьшился или остался тем же, "поднимаемся" вверх по дереву.
            while (stack.Count > 0 && stack.Peek().Level >= level)
            {
                stack.Pop();
            }

            if (stack.Count == 0)
            {
                roots.Add(node);
            }
            else
            {
                stack.Peek().Node.Children.Add(node);
            }

            stack.Push((level, node));
        }

        return roots;
    }
}
