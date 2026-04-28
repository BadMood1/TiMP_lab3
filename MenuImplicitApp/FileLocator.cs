using System;
using System.IO;

namespace MenuImplicitApp;

public static class FileLocator
{
    public static string FindFile(string fileName, int maxLevels = 10)
    {
        var baseDir = AppContext.BaseDirectory;

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

        var baseCandidate = Path.Combine(baseDir, fileName);
        return File.Exists(baseCandidate) ? Path.GetFullPath(baseCandidate) : Path.GetFullPath(baseCandidate);
    }
}
