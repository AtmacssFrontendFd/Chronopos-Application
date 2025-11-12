using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ChronoPos.Application.Logging
{
    /// <summary>
    /// Generalized logger class that can be used throughout the entire application.
    /// Logs are saved outside the src folder in a logs directory.
    /// </summary>
    public static class AppLogger
    {
        private static readonly object LockObject = new object();
        private static readonly string BaseLogDirectory;
        
        static AppLogger()
        {
            try
            {
                // Get the solution root directory (go up from any assembly location to find src folder)
                var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var srcPath = FindSrcDirectory(currentDirectory);
                
                if (srcPath != null)
                {
                    // Development environment: use solution root logs folder
                    var solutionRoot = Directory.GetParent(srcPath)?.FullName;
                    BaseLogDirectory = Path.Combine(solutionRoot ?? currentDirectory, "logs");
                }
                else
                {
                    // Production/Installed environment: use LocalApplicationData folder
                    // This avoids permission issues in Program Files and keeps logs with app data
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    BaseLogDirectory = Path.Combine(localAppData, "ChronoPos", "logs");
                }

                Console.WriteLine($"[AppLogger] Base Log Directory: {BaseLogDirectory}");
                
                if (!Directory.Exists(BaseLogDirectory))
                {
                    Directory.CreateDirectory(BaseLogDirectory);
                    Console.WriteLine($"[AppLogger] Created log directory: {BaseLogDirectory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppLogger] Error in static constructor: {ex.Message}");
                Console.WriteLine($"[AppLogger] Stack trace: {ex.StackTrace}");
                
                // Fallback to LocalApplicationData (safer than Program Files)
                try
                {
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    BaseLogDirectory = Path.Combine(localAppData, "ChronoPos", "logs");
                    
                    if (!Directory.Exists(BaseLogDirectory))
                    {
                        Directory.CreateDirectory(BaseLogDirectory);
                    }
                    Console.WriteLine($"[AppLogger] Using fallback log directory: {BaseLogDirectory}");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"[AppLogger] Fallback also failed: {fallbackEx.Message}");
                    // Last resort: use temp folder
                    BaseLogDirectory = Path.Combine(Path.GetTempPath(), "ChronoPos", "logs");
                    Directory.CreateDirectory(BaseLogDirectory);
                    Console.WriteLine($"[AppLogger] Using temp log directory: {BaseLogDirectory}");
                }
            }
        }

        /// <summary>
        /// Find the src directory by traversing up the directory tree
        /// </summary>
        private static string? FindSrcDirectory(string startPath)
        {
            var currentDir = new DirectoryInfo(startPath);
            
            while (currentDir != null)
            {
                // Check if this directory contains a 'src' folder
                var srcPath = Path.Combine(currentDir.FullName, "src");
                if (Directory.Exists(srcPath))
                {
                    return srcPath;
                }
                
                // Check if this directory IS the src folder
                if (currentDir.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    return currentDir.FullName;
                }
                
                currentDir = currentDir.Parent;
            }
            
            return null;
        }

        /// <summary>
        /// Log a message to the specified file with optional reason
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="reason">Optional reason or additional context</param>
        /// <param name="filename">The filename (without extension) to log to. Default is "application"</param>
        /// <param name="callerFilePath">Automatically captured caller file path</param>
        /// <param name="callerMemberName">Automatically captured caller member name</param>
        /// <param name="callerLineNumber">Automatically captured caller line number</param>
        public static void Log(string message, string? reason = null, string filename = "application",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            try
            {
                lock (LockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logFilePath = GetLogFilePath(filename);
                    
                    // Extract just the filename from the full path for cleaner logs
                    var sourceFile = Path.GetFileNameWithoutExtension(callerFilePath);
                    var location = $"{sourceFile}.{callerMemberName}:{callerLineNumber}";
                    
                    var logEntry = BuildLogEntry(timestamp, message, reason, location);
                    
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                    
                    // Also write to console for immediate feedback
                    Console.WriteLine($"[{timestamp}] [{filename}] {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppLogger] Error logging message: {ex.Message}");
            }
        }

        /// <summary>
        /// Log an error message with exception details
        /// </summary>
        public static void LogError(string message, Exception? exception = null, string? reason = null, string filename = "errors",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var errorMessage = $"ERROR: {message}";
            if (exception != null)
            {
                errorMessage += $" | Exception: {exception.Message}";
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    errorMessage += $" | StackTrace: {exception.StackTrace}";
                }
            }
            
            Log(errorMessage, reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        public static void LogInfo(string message, string? reason = null, string filename = "application",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Log($"INFO: {message}", reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void LogDebug(string message, string? reason = null, string filename = "debug",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Log($"DEBUG: {message}", reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string message, string? reason = null, string filename = "warnings",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Log($"WARNING: {message}", reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log a separator line for better readability
        /// </summary>
        public static void LogSeparator(string title = "", string filename = "application")
        {
            var separator = new string('=', 80);
            if (!string.IsNullOrEmpty(title))
            {
                Log($"{separator} {title} {separator}", filename: filename);
            }
            else
            {
                Log(separator, filename: filename);
            }
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        public static void LogPerformance(string operation, TimeSpan duration, string? reason = null, string filename = "performance",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var performanceMessage = $"PERFORMANCE: {operation} took {duration.TotalMilliseconds:F2}ms";
            Log(performanceMessage, reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log SQL queries for debugging
        /// </summary>
        public static void LogSql(string query, object? parameters = null, string filename = "sql",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var sqlMessage = $"SQL: {query}";
            var reason = parameters != null ? $"Parameters: {parameters}" : null;
            Log(sqlMessage, reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Get the full path for a log file
        /// </summary>
        private static string GetLogFilePath(string filename)
        {
            var dateString = DateTime.Now.ToString("yyyyMMdd");
            var logFileName = $"{filename}_{dateString}.log";
            return Path.Combine(BaseLogDirectory, logFileName);
        }

        /// <summary>
        /// Build a formatted log entry
        /// </summary>
        private static string BuildLogEntry(string timestamp, string message, string? reason, string location)
        {
            var entry = $"[{timestamp}] [{location}] {message}";
            
            if (!string.IsNullOrEmpty(reason))
            {
                entry += $" | Reason: {reason}";
            }
            
            return entry;
        }

        /// <summary>
        /// Get the current log directory path
        /// </summary>
        public static string GetLogDirectory()
        {
            return BaseLogDirectory;
        }

        /// <summary>
        /// Clean up old log files (older than specified days)
        /// </summary>
        public static void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(BaseLogDirectory))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(BaseLogDirectory, "*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                        Console.WriteLine($"[AppLogger] Deleted old log file: {fileInfo.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppLogger] Error cleaning up old logs: {ex.Message}");
            }
        }
    }
}