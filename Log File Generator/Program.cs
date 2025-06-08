using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

class LogFileGenerator
{
    private static Random _rand = new Random();
    private const string FileName = "sample.log";

    static void Main()
    {
        Console.WriteLine("=== Log File Generator ===");
        Console.Write("How many log entries would you like to generate? ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Invalid number.");
            return;
        }

        using (StreamWriter writer = new StreamWriter(FileName))
        {
            for (int i = 0; i < count; i++)
            {
                string logEntry = GenerateRandomLogEntry();
                writer.WriteLine(logEntry);
            }
        }

        Console.WriteLine($"\nGenerated {count} log entries in '{FileName}'.");
    }

    static string GenerateRandomLogEntry()
    {
        // Random date in last 7 days
        DateTime now = DateTime.Now;
        DateTime date = now.AddDays(-_rand.Next(7)).AddHours(-_rand.Next(24));
        date = date.AddMinutes(-_rand.Next(60)).AddSeconds(-_rand.Next(60));

        string level = RandomLevel();
        string message = RandomMessage(level);

        return $"{date:yyyy-MM-dd HH:mm:ss} {level} {message}";
    }

    static string RandomLevel()
    {
        string[] levels = { "INFO", "WARN", "ERROR" };
        return levels[_rand.Next(levels.Length)];
    }

    static string RandomMessage(string level)
    {
        Dictionary<string, string[]> messages = new Dictionary<string, string[]>
        {
            ["INFO"] = new[]
            {
                "User logged in",
                "System started",
                "Configuration loaded",
                "Data synchronization complete"
            },
            ["WARN"] = new[]
            {
                "Low disk space",
                "Memory usage high",
                "Deprecated API call detected",
                "Network latency detected"
            },
            ["ERROR"] = new[]
            {
                "Database connection failed",
                "Null reference exception",
                "Permission denied",
                "Configuration file not found"
            }
        };

        var list = messages[level];
        return list[_rand.Next(list.Length)];
    }
}