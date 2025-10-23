using System;
using System.IO;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop
{
    public static class LoggerDiagnostic
    {
        public static void DiagnoseLogging()
        {
            Console.WriteLine("=== LOGGER DIAGNOSTIC ===");
            Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            Console.WriteLine($"Directory.GetCurrentDirectory(): {Directory.GetCurrentDirectory()}");
            
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Console.WriteLine($"Expected Log Directory: {logDirectory}");
            Console.WriteLine($"Log Directory Exists: {Directory.Exists(logDirectory)}");
            
            // Test logging
            try
            {
                FileLogger.Log("=== DIAGNOSTIC TEST LOG ENTRY ===");
                Console.WriteLine("FileLogger.Log() executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileLogger.Log() failed: {ex.Message}");
            }
            
            // Check if directory was created
            Console.WriteLine($"Log Directory Exists After Test: {Directory.Exists(logDirectory)}");
            
            if (Directory.Exists(logDirectory))
            {
                var files = Directory.GetFiles(logDirectory, "*.log");
                Console.WriteLine($"Log files found: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"  - {file} (Size: {new FileInfo(file).Length} bytes)");
                    Console.WriteLine($"    Last Write: {new FileInfo(file).LastWriteTime}");
                }
            }
        }
    }
}