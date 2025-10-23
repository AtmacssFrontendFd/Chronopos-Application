using System;
using System.Runtime.CompilerServices;

namespace ChronoPos.Application.Logging
{
    /// <summary>
    /// Extension methods for easier logging
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Log this object's state or a message related to this object
        /// </summary>
        public static void LogThis(this object obj, string message, string? reason = null, string filename = "application",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var objectType = obj.GetType().Name;
            var fullMessage = $"[{objectType}] {message}";
            AppLogger.Log(fullMessage, reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Log an error related to this object
        /// </summary>
        public static void LogError(this object obj, string message, Exception? exception = null, string? reason = null, string filename = "errors",
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var objectType = obj.GetType().Name;
            var fullMessage = $"[{objectType}] {message}";
            AppLogger.LogError(fullMessage, exception, reason, filename, callerFilePath, callerMemberName, callerLineNumber);
        }

        /// <summary>
        /// Start a performance measurement
        /// </summary>
        public static PerformanceLogger StartPerformanceLog(this object obj, string operation, string filename = "performance")
        {
            var objectType = obj.GetType().Name;
            return new PerformanceLogger($"[{objectType}] {operation}", filename);
        }
    }

    /// <summary>
    /// Helper class for performance logging with automatic disposal
    /// </summary>
    public class PerformanceLogger : IDisposable
    {
        private readonly string _operation;
        private readonly string _filename;
        private readonly DateTime _startTime;
        private readonly string _callerFilePath;
        private readonly string _callerMemberName;
        private readonly int _callerLineNumber;

        public PerformanceLogger(string operation, string filename,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            _operation = operation;
            _filename = filename;
            _startTime = DateTime.Now;
            _callerFilePath = callerFilePath;
            _callerMemberName = callerMemberName;
            _callerLineNumber = callerLineNumber;
            
            AppLogger.Log($"PERFORMANCE START: {_operation}", filename: _filename, callerFilePath: _callerFilePath, 
                callerMemberName: _callerMemberName, callerLineNumber: _callerLineNumber);
        }

        public void Dispose()
        {
            var duration = DateTime.Now - _startTime;
            AppLogger.LogPerformance(_operation, duration, filename: _filename, callerFilePath: _callerFilePath, 
                callerMemberName: _callerMemberName, callerLineNumber: _callerLineNumber);
        }
    }
}