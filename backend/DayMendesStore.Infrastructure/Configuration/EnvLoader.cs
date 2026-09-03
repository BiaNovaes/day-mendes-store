using System;
using System.IO;

namespace DayMendesStore.Infrastructure.Configuration;

public static class EnvLoader
{
    private static bool _loaded = false;

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);

        string? envPath = FindEnvFile(currentDir) ?? FindEnvFile(baseDir);

        if (envPath == null || !File.Exists(envPath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Nunca sobrescreve uma variável de ambiente que já exista no sistema
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindEnvFile(DirectoryInfo? startDir)
    {
        var dir = startDir;
        for (int i = 0; i < 5 && dir != null; i++)
        {
            var testPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(testPath))
            {
                return testPath;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
