using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

class TaskSchedulerApp
{
    private static List<ScheduledTask> tasks = new List<ScheduledTask>();
    private const string FileName = "tasks.dat";

    static void Main()
    {
        LoadTasks();

        Console.WriteLine("=== Task Scheduler ===");
        Console.WriteLine("1. Add Task");
        Console.WriteLine("2. View Tasks");
        Console.WriteLine("3. Exit");

        while (true)
        {
            Console.Write("\nChoose an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddTask();
                    break;
                case "2":
                    ViewTasks();
                    break;
                case "3":
                    SaveTasks();
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    static void AddTask()
    {
        Console.Write("Enter task name: ");
        string name = Console.ReadLine();

        Console.Write("Enter interval in seconds: ");
        if (!int.TryParse(Console.ReadLine(), out int interval))
        {
            Console.WriteLine("Invalid interval.");
            return;
        }

        var task = new ScheduledTask(name, interval * 1000); // convert to milliseconds
        task.Start();
        tasks.Add(task);

        Console.WriteLine($"Task '{name}' scheduled every {interval} seconds.");
    }

    static void ViewTasks()
    {
        Console.WriteLine("\nScheduled Tasks:");
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks scheduled.");
            return;
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tasks[i]}");
        }
    }

    static void LoadTasks()
    {
        if (!File.Exists(FileName)) return;

        foreach (string line in File.ReadAllLines(FileName))
        {
            string[] parts = line.Split('|');
            if (parts.Length == 2 && double.TryParse(parts[1], out double interval))
            {
                var task = new ScheduledTask(parts[0], interval);
                task.Start();
                tasks.Add(task);
            }
        }
    }

    static void SaveTasks()
    {
        List<string> lines = new List<string>();
        foreach (var task in tasks)
        {
            lines.Add($"{task.Name}|{task.Interval}");
        }
        File.WriteAllLines(FileName, lines);
    }
}

public class ScheduledTask
{
    public string Name { get; }
    public double Interval => timer.Interval;
    private Timer timer;

    public ScheduledTask(string name, double interval)
    {
        Name = name;
        timer = new Timer(interval);
        timer.Elapsed += OnTimerElapsed;
    }

    public void Start()
    {
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    public void Stop()
    {
        timer.Enabled = false;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Console.WriteLine($"[{e.SignalTime}] Task '{Name}' triggered.");
    }

    public override string ToString()
    {
        return $"{Name} (every {(timer.Interval / 1000):F0}s)";
    }
}