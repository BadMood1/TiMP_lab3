using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuthorizationLibrary;

/// Сервис авторизации по внешнему файлу USERS.
/// Логика доступа к пунктам меню задаётся данными, поэтому приложение можно настраивать без перекомпиляции.
public sealed class AuthorizationService
{
    private readonly string _usersFilePath;

    public AuthorizationService(string usersFileName = "USERS.txt")
    {
        if (string.IsNullOrWhiteSpace(usersFileName))
        {
            throw new ArgumentException("Имя файла USERS не задано.", nameof(usersFileName));
        }

        _usersFilePath = usersFileName;
        if (!Path.IsPathRooted(usersFileName))
        {
            _usersFilePath = Path.Combine(AppContext.BaseDirectory, usersFileName);
        }

        if (!File.Exists(_usersFilePath))
        {
            throw new FileNotFoundException($"Файл USERS не найден: {_usersFilePath}", _usersFilePath);
        }
    }

    /// Проверяет логин/пароль и возвращает словарь статусов пунктов меню (title -> status).
    /// Возвращает null, если аутентификация не пройдена.
    /// Статусы соответствуют методичке: 0 - виден и доступен, 1 - виден, но недоступен, 2 - скрыт.
    public Dictionary<string, int>? Authenticate(string username, string password)
    {
        var normalizedUsername = (username ?? string.Empty).Trim();
        var normalizedPassword = (password ?? string.Empty).Trim();
        if (normalizedUsername.Length == 0)
        {
            return null;
        }

        // Загружаем всех пользователей и их права из файла USERS.
        var users = LoadUsers();

        if (!users.TryGetValue(normalizedUsername, out var record))
        {
            return null;
        }

        if (!string.Equals(record.Password, normalizedPassword, StringComparison.Ordinal))
        {
            return null;
        }

        // Возвращаем копию, чтобы вызывающая сторона не могла повлиять на внутреннее состояние.
        return new Dictionary<string, int>(record.Statuses, StringComparer.Ordinal);
    }

    /// Полностью читает файл USERS и формирует структуру:
    /// имя пользователя -> (пароль, словарь статусов пунктов меню).
    private Dictionary<string, (string Password, Dictionary<string, int> Statuses)> LoadUsers()
    {
        // Методика допускает редактирование файла любым редактором — поэтому кодировка может быть разной.
        // Сначала пробуем «строгий» UTF-8, при ошибке откатываемся к системной кодировке.
        var lines = ReadAllLinesWithFallback(_usersFilePath);

        var result = new Dictionary<string, (string Password, Dictionary<string, int> Statuses)>(StringComparer.Ordinal);
        string? currentUser = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                // Формат: #Имя_пользователя Пароль
                var withoutHash = line.TrimStart('#').Trim();
                var parts = withoutHash.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    currentUser = null;
                    continue;
                }

                var username = parts[0];
                var password = parts[1];
                result[username] = (password, new Dictionary<string, int>(StringComparer.Ordinal));
                currentUser = username;
                continue;
            }

            if (currentUser == null)
            {
                continue;
            }

            // Формат: Название_пункта Статус
            var parts2 = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts2.Length < 2)
            {
                continue;
            }

            var statusToken = parts2[^1];
            if (!int.TryParse(statusToken, out var status))
            {
                continue;
            }

            var title = string.Join(" ", parts2.Take(parts2.Length - 1));
            if (title.Length == 0)
            {
                continue;
            }

            result[currentUser].Statuses[title] = status;
        }

        return result;
    }

    private static string[] ReadAllLinesWithFallback(string path)
    {
        try
        {
            return File.ReadAllLines(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
        catch
        {
            return File.ReadAllLines(path, Encoding.Default);
        }
    }
}
