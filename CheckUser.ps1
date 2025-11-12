# Quick script to check if user ID 1 exists
$dbPath = "$env:LOCALAPPDATA\ChronoPos\chronopos.db"

# Create a simple C# program to query SQLite
$code = @"
using System;
using System.Data.SQLite;

public class DbCheck {
    public static void Main() {
        var conn = new SQLiteConnection("Data Source=$($dbPath.Replace('\', '\\'))");
        try {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT user_id, user_name FROM users WHERE user_id = 1;";
            var reader = cmd.ExecuteReader();
            if (reader.Read()) {
                Console.WriteLine("User ID 1 exists: " + reader["user_name"]);
            } else {
                Console.WriteLine("ERROR: User ID 1 does not exist!");
            }
            reader.Close();
            
            // Check all users
            cmd.CommandText = "SELECT user_id, user_name FROM users LIMIT 10;";
            reader = cmd.ExecuteReader();
            Console.WriteLine("\nAll users:");
            while (reader.Read()) {
                Console.WriteLine("  ID: " + reader["user_id"] + ", Name: " + reader["user_name"]);
            }
        } finally {
            conn.Close();
        }
    }
}
"@

# Compile and run
Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Data.SQLite.dll" -Language CSharp -ErrorAction SilentlyContinue
[DbCheck]::Main()
