using ChronoPos.Application.Interfaces;
using System.IO;

namespace ChronoPos.Application.Services;

/// <summary>
/// File-based logging service for Application layer
/// </summary>
public class ApplicationLoggingService : ILoggingService
{
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string LogFilePath = Path.Combine(LogDirectory, $"application_{DateTime.Now:yyyyMMdd}.log");
    private static readonly object LockObject = new object();

    static ApplicationLoggingService()
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
    }

    /// <summary>
    /// Logs a message to the application log file
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Log(string message)
    {
        try
        {
            lock (LockObject)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] {message}";
                
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                
                // Also write to console/debug output for immediate feedback
                Console.WriteLine(logEntry);
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Logging error: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a separator line with optional title
    /// </summary>
    /// <param name="title">Optional title for the separator</param>
    public void LogSeparator(string title = "")
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

    /// <summary>
    /// Logs an error message with optional exception details
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="exception">Optional exception details</param>
    public void LogError(string message, Exception? exception = null)
    {
        Log($"❌ ERROR: {message}");
        if (exception != null)
        {
            Log($"❌ Exception Type: {exception.GetType().Name}");
            Log($"❌ Exception Message: {exception.Message}");
            Log($"❌ Stack Trace: {exception.StackTrace}");
            
            if (exception.InnerException != null)
            {
                Log($"❌ Inner Exception: {exception.InnerException.Message}");
            }
        }
    }
}
