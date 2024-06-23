using System.Data.SQLite;

namespace SQLite.Test;

class Program
{
    static void Main(string[] args)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", "chronojump.db");
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }
        string connectionString = $"Version=3;Data Source={dbPath}";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            var createTableQuery = @"CREATE TABLE IF NOT EXISTS MyTable (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
            Console.WriteLine("Database created.");
        }
        Console.ReadLine();
    }
}

