using System;
using System.IO;

namespace ChronoPos.Application.Logging
{
    /// <summary>
    /// Configuration options for the AppLogger
    /// </summary>
    public static class LoggerConfig
    {
        private static LogLevel _minimumLogLevel = LogLevel.Info;
        private static bool _enableConsoleOutput = true;
        private static bool _enableFileOutput = true;
        private static string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private static int _maxLogFileSizeMB = 50;
        private static int _logRetentionDays = 30;

        /// <summary>
        /// Minimum log level to record
        /// </summary>
        public static LogLevel MinimumLogLevel
        {
            get => _minimumLogLevel;
            set => _minimumLogLevel = value;
        }

        /// <summary>
        /// Enable or disable console output
        /// </summary>
        public static bool EnableConsoleOutput
        {
            get => _enableConsoleOutput;
            set => _enableConsoleOutput = value;
        }

        /// <summary>
        /// Enable or disable file output
        /// </summary>
        public static bool EnableFileOutput
        {
            get => _enableFileOutput;
            set => _enableFileOutput = value;
        }

        /// <summary>
        /// DateTime format for log entries
        /// </summary>
        public static string DateTimeFormat
        {
            get => _dateTimeFormat;
            set => _dateTimeFormat = value ?? "yyyy-MM-dd HH:mm:ss.fff";
        }

        /// <summary>
        /// Maximum log file size in MB before rotation
        /// </summary>
        public static int MaxLogFileSizeMB
        {
            get => _maxLogFileSizeMB;
            set => _maxLogFileSizeMB = Math.Max(1, value);
        }

        /// <summary>
        /// Number of days to retain log files
        /// </summary>
        public static int LogRetentionDays
        {
            get => _logRetentionDays;
            set => _logRetentionDays = Math.Max(1, value);
        }

        /// <summary>
        /// Check if a log level should be recorded based on minimum log level
        /// </summary>
        public static bool ShouldLog(LogLevel level)
        {
            return level >= _minimumLogLevel;
        }
    }

    /// <summary>
    /// Log levels in order of severity
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}