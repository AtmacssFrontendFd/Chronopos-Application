namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Interface for logging service
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs a message
    /// </summary>
    /// <param name="message">The message to log</param>
    void Log(string message);

    /// <summary>
    /// Logs a separator with optional title
    /// </summary>
    /// <param name="title">Optional title for the separator</param>
    void LogSeparator(string title = "");

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="exception">Optional exception details</param>
    void LogError(string message, Exception? exception = null);
}
