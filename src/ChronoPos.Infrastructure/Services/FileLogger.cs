using System;
using System.IO;

namespace ChronoPos.Infrastructure.Services
{
    public static class FileLogger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"stock_adjustment_service_{DateTime.Now:yyyyMMdd}.log");
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
            lock (LockObject)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] {message}";
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // If logging fails, write to console as fallback
                    Console.WriteLine($"[LOGGING ERROR] {ex.Message}: {message}");
                }
            }
        }

        public static void LogSeparator(string title)
        {
            Log($"================================================== {title} ==================================================");
        }

        public static void LogError(string message, Exception? ex = null)
        {
            Log($"ERROR: {message}");
            if (ex != null)
            {
                Log($"Exception: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
            }
        }

        public static void LogInfo(string message)
        {
            Log($"INFO: {message}");
        }

        public static void LogDebug(string message)
        {
            Log($"DEBUG: {message}");
        }

        public static void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }
    }
}
