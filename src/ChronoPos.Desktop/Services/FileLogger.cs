using System;
using System.IO;

namespace ChronoPos.Desktop.Services
{
    public static class FileLogger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"desktop_ui_{DateTime.Now:yyyyMMdd}.log");
        private static readonly object LockObject = new object();

        static FileLogger()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void Log(string message)
        {
            try
            {
                lock (LockObject)
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] {message}";
                    
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                    
                    // Also write to console for immediate feedback
                    Console.WriteLine(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        public static void LogSeparator(string title = "")
        {
            var separator = new string('=', 50);
            if (!string.IsNullOrEmpty(title))
            {
                Log($"{separator} {title} {separator}");
            }
            else
            {
                Log(separator);
            }
        }
    }
}
