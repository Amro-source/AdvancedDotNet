using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class LogAnalyzerApp
{
    static void Main()
    {
        Console.WriteLine("=== Log Analyzer ===");
        Console.Write("Enter log file path: ");
        string filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Error: File not found.");
            return;
        }

        // Initialize counters
        var levelCounts = new Dictionary<string, int>();
        var dateCounts = new Dictionary<string, int>();

        // Sample log line pattern: "2025-04-01 10:15:30 INFO User logged in"
        Regex regex = new Regex(@"^(?<date>\d{4}-\d{2}-\d{2}) \d{2}:\d{2}:\d{2} (?<level>\w+)");

        int totalLines = 0;
        int matchedLines = 0;

        foreach (string line in File.ReadLines(filePath))
        {
            totalLines++;
            var match = regex.Match(line);
            if (match.Success)
            {
                matchedLines++;
                string level = match.Groups["level"].Value.ToUpper();
                string date = match.Groups["date"].Value;

                UpdateDictionary(levelCounts, level);
                UpdateDictionary(dateCounts, date);
            }
        }

        Console.WriteLine($"\nAnalyzed {matchedLines} out of {totalLines} lines.\n");

        if (levelCounts.Count > 0)
        {
            Console.WriteLine("Log Level Summary:");
            foreach (var entry in levelCounts)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }
        else
        {
            Console.WriteLine("No matching log levels found.");
        }

        if (dateCounts.Count > 0)
        {
            Console.WriteLine("\nDate Summary:");
            foreach (var entry in dateCounts)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }
    }

    static void UpdateDictionary(Dictionary<string, int> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            dict[key]++;
        }
        else
        {
            dict[key] = 1;
        }
    }
}