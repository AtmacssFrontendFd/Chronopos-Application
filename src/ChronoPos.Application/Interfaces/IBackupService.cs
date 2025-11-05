using System;
using System.Threading.Tasks;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service for database backup operations
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Backup database to specified path
    /// </summary>
    Task<bool> BackupDatabaseAsync(string destinationPath);

    /// <summary>
    /// Get source database path
    /// </summary>
    string GetSourceDatabasePath();

    /// <summary>
    /// Validate backup file path
    /// </summary>
    bool ValidateBackupPath(string path);

    /// <summary>
    /// Get default backup path
    /// </summary>
    string GetDefaultBackupPath();

    /// <summary>
    /// Start automatic backup scheduler
    /// </summary>
    void StartBackupScheduler(string backupPath, string frequency);

    /// <summary>
    /// Stop automatic backup scheduler
    /// </summary>
    void StopBackupScheduler();

    /// <summary>
    /// Get last backup information
    /// </summary>
    Task<BackupInfo?> GetLastBackupInfoAsync(string backupPath);

    /// <summary>
    /// Stage a database restore (will be applied on next app start)
    /// </summary>
    Task<RestoreResult> StageRestoreAsync(string backupFilePath);

    /// <summary>
    /// Check if there's a pending restore staged
    /// </summary>
    Task<bool> HasPendingRestoreAsync();

    /// <summary>
    /// Apply pending restore (called at app startup before DB initialization)
    /// </summary>
    Task<RestoreResult> ApplyPendingRestoreAsync();

    /// <summary>
    /// Cancel pending restore
    /// </summary>
    Task<bool> CancelPendingRestoreAsync();

    /// <summary>
    /// Validate backup file for restore (check if valid SQLite DB)
    /// </summary>
    Task<BackupValidationResult> ValidateBackupFileAsync(string backupFilePath);
}

/// <summary>
/// Information about a backup
/// </summary>
public class BackupInfo
{
    public DateTime BackupTime { get; set; }
    public long FileSize { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a restore operation
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PreRestoreBackupPath { get; set; }
    public string? ErrorDetails { get; set; }
}

/// <summary>
/// Result of backup file validation
/// </summary>
public class BackupValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TableCount { get; set; }
    public long FileSize { get; set; }
}
