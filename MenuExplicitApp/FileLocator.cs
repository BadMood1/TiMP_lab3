using System;
using System.IO;

namespace MenuExplicitApp;

public static class FileLocator
{
    public static string FindFile(string fileName, int maxLevels = 10)
    {
        var baseDir = AppContext.BaseDirectory;

        // Search parent directories first (prefer project/source files over output copy)
        var dir = new DirectoryInfo(baseDir).Parent;
        for (int i = 0; i < maxLevels && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
            dir = dir.Parent;
        }

        // Fallback to file in the app base directory (output folder)
        var baseCandidate = Path.Combine(baseDir, fileName);
        return File.Exists(baseCandidate) ? Path.GetFullPath(baseCandidate) : Path.GetFullPath(baseCandidate);
    }
}
