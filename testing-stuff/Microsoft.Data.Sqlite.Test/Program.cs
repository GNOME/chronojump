using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace Microsoft.Data.Sqlite.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
                "Chronojump",
                "database",
                "chronojump.db");
            //var dbDir = Path.GetDirectoryName(dbPath);
            var dbDir = "/Users/joeries/.local/share/Chronojump/database/chronojump.db";
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            var connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = dbPath
            }.ToString();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var createTableSql = @"CREATE TABLE IF NOT EXISTS MyTable (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)";
                using (var command = new SqliteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                var queryTableSql = "SELECT person77.name FROM person77, jump, personSession77 WHERE person77.uniqueID == 2 AND person77.uniqueID == jump.personID";
                using (var command = new SqliteCommand(queryTableSql, connection))
                {
                    command.ExecuteReader();
                }

                queryTableSql = "SELECT person77.name, person77.sex, jump.sessionID, jump.tv, jump.weight, personSession77.weight, jump.type FROM jump, person77, personSession77, jumpType WHERE ( jump.sessionID = 4) AND jumpType.startIn = 1 AND jump.Type = jumpType.name AND jump.personID = person77.uniqueID AND person77.uniqueID = personSession77.personID AND jump.sessionID = personSession77.sessionID ORDER BY jump.tv DESC";
                using (var command = new SqliteCommand(queryTableSql, connection))
                {
                    command.ExecuteReader();
                }

                queryTableSql = "SELECT jump.type, jumpType.startIn FROM jump, jumpType WHERE jump.Type = jumpType.name";
                using (var command = new SqliteCommand(queryTableSql, connection))
                {
                    command.ExecuteReader();
                }

                connection.Close();
                Console.WriteLine("Table created.");
            }
            Console.ReadLine();
        }
    }
}
