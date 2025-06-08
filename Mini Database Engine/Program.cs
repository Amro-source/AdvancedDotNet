using System;
using System.Collections.Generic;
using System.Linq;

class MiniDatabaseEngine
{
    static void Main()
    {
        Database db = new Database();

        Console.WriteLine("=== Mini Database Engine ===");
        Console.WriteLine("Supported Commands:");
        Console.WriteLine("CREATE TABLE table_name");
        Console.WriteLine("INSERT INTO table_name VALUES field1=value1, field2=value2");
        Console.WriteLine("SELECT * FROM table_name [WHERE field=value]");
        Console.WriteLine("UPDATE table_name SET field=value [WHERE field=value]");
        Console.WriteLine("DELETE FROM table_name [WHERE field=value]");
        Console.WriteLine("Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine().Trim();

            if (input.ToLower() == "exit") break;

            try
            {
                string result = db.Execute(input);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}

public class Database
{
    private Dictionary<string, Table> tables = new Dictionary<string, Table>();

    public string Execute(string command)
    {
        command = command.Trim();
        if (command.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
        {
            string tableName = ExtractTableName(command, "CREATE TABLE");
            if (tables.ContainsKey(tableName))
                throw new InvalidOperationException($"Table '{tableName}' already exists.");

            tables[tableName] = new Table();
            return $"Table '{tableName}' created.";
        }
        else if (command.StartsWith("INSERT INTO", StringComparison.OrdinalIgnoreCase))
        {
            int valuesIndex = command.IndexOf("VALUES", StringComparison.OrdinalIgnoreCase);
            if (valuesIndex == -1)
                throw new ArgumentException("Missing 'VALUES' keyword.");

            string tableName = command.Substring("INSERT INTO".Length, valuesIndex - "INSERT INTO".Length).Trim();
            string valuesPart = command.Substring(valuesIndex + "VALUES".Length).Trim();

            if (!tables.TryGetValue(tableName, out Table table))
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");

            var rowData = ParseFields(valuesPart);
            table.Insert(rowData);
            return $"1 row inserted into '{tableName}'.";
        }
        else if (command.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string tableName = parts[parts.Length >= 4 ? 3 : 2];

            if (!tables.TryGetValue(tableName, out Table table))
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");

            Dictionary<string, string> whereClause = null;
            if (command.Contains("WHERE"))
            {
                int whereIndex = command.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
                string wherePart = command.Substring(whereIndex + "WHERE".Length).Trim();
                whereClause = ParseWhereClause(wherePart);
            }

            List<Dictionary<string, string>> results = table.Select(whereClause);
            return FormatResults(results);
        }
        else if (command.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string tableName = parts[1];

            if (!tables.TryGetValue(tableName, out Table table))
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");

            int setIndex = command.IndexOf("SET", StringComparison.OrdinalIgnoreCase);
            int whereIndex = command.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);

            string setPart = whereIndex == -1
                ? command.Substring(setIndex + 3).Trim()
                : command.Substring(setIndex + 3, whereIndex - setIndex - 3).Trim();

            var updateData = ParseFields(setPart);

            Dictionary<string, string> whereClause = null;
            if (whereIndex != -1)
            {
                string wherePart = command.Substring(whereIndex + 5).Trim();
                whereClause = ParseWhereClause(wherePart);
            }

            int count = table.Update(updateData, whereClause);
            return $"{count} row(s) updated.";
        }
        else if (command.StartsWith("DELETE FROM", StringComparison.OrdinalIgnoreCase))
        {
            string tableName = ExtractTableName(command, "DELETE FROM");

            if (!tables.TryGetValue(tableName, out Table table))
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");

            Dictionary<string, string> whereClause = null;
            if (command.Contains("WHERE"))
            {
                int whereIndex = command.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
                string wherePart = command.Substring(whereIndex + 5).Trim();
                whereClause = ParseWhereClause(wherePart);
            }

            int count = table.Delete(whereClause);
            return $"{count} row(s) deleted.";
        }
        else
        {
            throw new ArgumentException("Unrecognized command.");
        }
    }

    private string ExtractTableName(string command, string prefix)
    {
        string tableName = command.Substring(prefix.Length).Trim();
        if (tableName.Contains(' '))
        {
            tableName = tableName.Substring(0, tableName.IndexOf(' '));
        }
        return tableName;
    }

    private Dictionary<string, string> ParseFields(string part)
    {
        var result = new Dictionary<string, string>();
        foreach (var pair in part.Split(','))
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length != 2)
                throw new ArgumentException("Invalid field format: " + pair);
            result[keyValue[0].Trim()] = keyValue[1].Trim();
        }
        return result;
    }

    private Dictionary<string, string> ParseWhereClause(string part)
    {
        string[] conditions = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
        if (conditions.Length < 2)
            throw new ArgumentException("Invalid WHERE clause.");
        return new Dictionary<string, string>
        {
            { conditions[0].Trim(), conditions[1].Trim() }
        };
    }

    private string FormatResults(List<Dictionary<string, string>> results)
    {
        if (results.Count == 0) return "No results found.";

        string header = string.Join(" | ", results[0].Keys);
        string separator = string.Join("-", header.Select(c => "-"));
        string body = string.Join("\n", results.Select(r => string.Join(" | ", r.Values)));

        return $"{header}\n{separator}\n{body}";
    }
}

public class Table
{
    private List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

    public void Insert(Dictionary<string, string> data)
    {
        rows.Add(new Dictionary<string, string>(data));
    }

    public List<Dictionary<string, string>> Select(Dictionary<string, string> whereClause)
    {
        return whereClause == null
            ? rows
            : rows.FindAll(row => MatchRow(row, whereClause));
    }

    public int Update(Dictionary<string, string> data, Dictionary<string, string> whereClause)
    {
        int count = 0;
        foreach (var row in rows)
        {
            if (whereClause == null || MatchRow(row, whereClause))
            {
                foreach (var kv in data)
                {
                    if (row.ContainsKey(kv.Key))
                        row[kv.Key] = kv.Value;
                    else
                        row[kv.Key] = kv.Value;
                }
                count++;
            }
        }
        return count;
    }

    public int Delete(Dictionary<string, string> whereClause)
    {
        int count = 0;
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            if (whereClause == null || MatchRow(rows[i], whereClause))
            {
                rows.RemoveAt(i);
                count++;
            }
        }
        return count;
    }

    private bool MatchRow(Dictionary<string, string> row, Dictionary<string, string> condition)
    {
        foreach (var kv in condition)
        {
            if (!row.TryGetValue(kv.Key, out string value) || value != kv.Value)
                return false;
        }
        return true;
    }
}
