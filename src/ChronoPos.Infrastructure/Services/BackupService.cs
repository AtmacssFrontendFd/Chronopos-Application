using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChronoPos.Application.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service for database backup operations
/// </summary>
public class BackupService : IBackupService, IDisposable
{
    private readonly ILogger<BackupService> _logger;
    private Timer? _backupTimer;
    private string? _backupPath;
    private string? _frequency;

    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get source database path
    /// </summary>
    public string GetSourceDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "ChronoPos", "chronopos.db");
    }

    /// <summary>
    /// Get default backup path
    /// </summary>
    public string GetDefaultBackupPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var backupFolder = Path.Combine(localAppData, "ChronoPos", "Backups");
        
        // Ensure backup folder exists
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }

        return Path.Combine(backupFolder, "chronopos_backup.db");
    }

    /// <summary>
    /// Validate backup file path
    /// </summary>
    public bool ValidateBackupPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // Check if directory exists or can be created
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
                return false;

            // Check if path is valid
            Path.GetFullPath(path);

            // Check if we have write permissions (try to create directory if it doesn't exist)
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Backup database to specified path
    /// </summary>
    public async Task<bool> BackupDatabaseAsync(string destinationPath)
    {
        try
        {
            var sourcePath = GetSourceDatabasePath();

            if (!File.Exists(sourcePath))
            {
                _logger.LogError("Source database file not found: {SourcePath}", sourcePath);
                return false;
            }

            if (!ValidateBackupPath(destinationPath))
            {
                _logger.LogError("Invalid backup path: {DestinationPath}", destinationPath);
                return false;
            }

            // Ensure destination directory exists
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Copy database file
            await Task.Run(() =>
            {
                // If destination exists, delete it first (overwrite)
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Copy(sourcePath, destinationPath, overwrite: true);
            });

            _logger.LogInformation("Database backed up successfully to: {DestinationPath}", destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up database to: {DestinationPath}", destinationPath);
            return false;
        }
    }

    /// <summary>
    /// Get last backup information
    /// </summary>
    public Task<BackupInfo?> GetLastBackupInfoAsync(string backupPath)
    {
        return Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
                    return null;

                var fileInfo = new FileInfo(backupPath);

                return new BackupInfo
                {
                    BackupTime = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup info for: {BackupPath}", backupPath);
                return null;
            }
        });
    }

    /// <summary>
    /// Start automatic backup scheduler
    /// </summary>
    public void StartBackupScheduler(string backupPath, string frequency)
    {
        StopBackupScheduler();

        _backupPath = backupPath;
        _frequency = frequency;

        var interval = GetIntervalFromFrequency(frequency);
        if (interval == TimeSpan.Zero)
        {
            _logger.LogInformation("Backup frequency set to Manual - no automatic backups will run");
            return;
        }

        _backupTimer = new Timer(
            async _ => await PerformScheduledBackupAsync(),
            null,
            interval,
            interval
        );

        _logger.LogInformation("Backup scheduler started with frequency: {Frequency}, interval: {Interval}", 
            frequency, interval);
    }

    /// <summary>
    /// Stop automatic backup scheduler
    /// </summary>
    public void StopBackupScheduler()
    {
        _backupTimer?.Dispose();
        _backupTimer = null;
        _logger.LogInformation("Backup scheduler stopped");
    }

    /// <summary>
    /// Perform scheduled backup
    /// </summary>
    private async Task PerformScheduledBackupAsync()
    {
        if (string.IsNullOrWhiteSpace(_backupPath))
            return;

        try
        {
            _logger.LogInformation("Starting scheduled backup to: {BackupPath}", _backupPath);
            var success = await BackupDatabaseAsync(_backupPath);
            
            if (success)
            {
                _logger.LogInformation("Scheduled backup completed successfully");
            }
            else
            {
                _logger.LogError("Scheduled backup failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled backup");
        }
    }

    /// <summary>
    /// Get time interval from frequency string
    /// </summary>
    private TimeSpan GetIntervalFromFrequency(string frequency)
    {
        return frequency?.ToLower() switch
        {
            "hourly" => TimeSpan.FromHours(1),
            "daily" => TimeSpan.FromDays(1),
            "weekly" => TimeSpan.FromDays(7),
            "monthly" => TimeSpan.FromDays(30),
            "manual" => TimeSpan.Zero,
            _ => TimeSpan.Zero
        };
    }

    #region Restore Operations

    /// <summary>
    /// Get pending restore directory path
    /// </summary>
    private string GetPendingRestoreDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "ChronoPos", "PendingRestore");
    }

    /// <summary>
    /// Get pending restore metadata file path
    /// </summary>
    private string GetPendingRestoreMetadataPath()
    {
        return Path.Combine(GetPendingRestoreDirectory(), "restore_pending.json");
    }

    /// <summary>
    /// Get staged backup copy path
    /// </summary>
    private string GetStagedBackupPath()
    {
        return Path.Combine(GetPendingRestoreDirectory(), "backup_staged.db");
    }

    /// <summary>
    /// Validate backup file for restore
    /// </summary>
    public async Task<BackupValidationResult> ValidateBackupFileAsync(string backupFilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    Message = "Backup file path is empty."
                };
            }

            if (!File.Exists(backupFilePath))
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    Message = "Backup file does not exist."
                };
            }

            var fileInfo = new FileInfo(backupFilePath);
            if (fileInfo.Length < 1024) // Less than 1KB is suspicious
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    Message = "Backup file is too small to be a valid database.",
                    FileSize = fileInfo.Length
                };
            }

            // Try to open as SQLite database
            int tableCount = 0;
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection($"Data Source={backupFilePath};Mode=ReadOnly");
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                tableCount = Convert.ToInt32(command.ExecuteScalar());
            });

            if (tableCount == 0)
            {
                return new BackupValidationResult
                {
                    IsValid = false,
                    Message = "Backup file appears to be empty (no tables found).",
                    FileSize = fileInfo.Length,
                    TableCount = tableCount
                };
            }

            return new BackupValidationResult
            {
                IsValid = true,
                Message = $"Valid backup file with {tableCount} tables.",
                FileSize = fileInfo.Length,
                TableCount = tableCount
            };
        }
        catch (SqliteException ex)
        {
            return new BackupValidationResult
            {
                IsValid = false,
                Message = $"Invalid SQLite database file: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating backup file: {BackupFilePath}", backupFilePath);
            return new BackupValidationResult
            {
                IsValid = false,
                Message = $"Error validating backup: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Stage a database restore (will be applied on next app start)
    /// </summary>
    public async Task<RestoreResult> StageRestoreAsync(string backupFilePath)
    {
        try
        {
            _logger.LogInformation("Starting staged restore from: {BackupFilePath}", backupFilePath);

            // Validate backup file
            var validation = await ValidateBackupFileAsync(backupFilePath);
            if (!validation.IsValid)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "Backup file validation failed.",
                    ErrorDetails = validation.Message
                };
            }

            // Create pending restore directory
            var pendingDir = GetPendingRestoreDirectory();
            if (!Directory.Exists(pendingDir))
            {
                Directory.CreateDirectory(pendingDir);
            }

            // Copy backup to staging area
            var stagedPath = GetStagedBackupPath();
            _logger.LogInformation("Copying backup to staging area: {StagedPath}", stagedPath);
            
            await Task.Run(() =>
            {
                if (File.Exists(stagedPath))
                {
                    File.Delete(stagedPath);
                }
                File.Copy(backupFilePath, stagedPath, overwrite: true);
            });

            // Create metadata
            var metadata = new RestoreMetadata
            {
                OriginalBackupPath = backupFilePath,
                StagedBackupPath = stagedPath,
                RequestedAt = DateTime.UtcNow,
                BackupFileSize = validation.FileSize,
                BackupTableCount = validation.TableCount
            };

            var metadataPath = GetPendingRestoreMetadataPath();
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, json);

            _logger.LogInformation("Restore staged successfully. Restart application to apply.");

            return new RestoreResult
            {
                Success = true,
                Message = "Restore staged successfully. Please restart the application to apply the restore.",
                PreRestoreBackupPath = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staging restore");
            return new RestoreResult
            {
                Success = false,
                Message = "Error staging restore.",
                ErrorDetails = ex.Message
            };
        }
    }

    /// <summary>
    /// Check if there's a pending restore
    /// </summary>
    public Task<bool> HasPendingRestoreAsync()
    {
        return Task.Run(() =>
        {
            var metadataPath = GetPendingRestoreMetadataPath();
            var stagedPath = GetStagedBackupPath();
            return File.Exists(metadataPath) && File.Exists(stagedPath);
        });
    }

    /// <summary>
    /// Apply pending restore (called at app startup before DB initialization)
    /// </summary>
    public async Task<RestoreResult> ApplyPendingRestoreAsync()
    {
        try
        {
            var metadataPath = GetPendingRestoreMetadataPath();
            var stagedPath = GetStagedBackupPath();

            if (!File.Exists(metadataPath) || !File.Exists(stagedPath))
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "No pending restore found.",
                    ErrorDetails = "Metadata or staged backup file missing."
                };
            }

            // Read metadata
            var json = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<RestoreMetadata>(json);

            if (metadata == null)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "Failed to read restore metadata.",
                    ErrorDetails = "Metadata deserialization failed."
                };
            }

            _logger.LogInformation("Applying pending restore from: {OriginalPath}", metadata.OriginalBackupPath);

            // Get current database path
            var currentDbPath = GetSourceDatabasePath();

            // Create pre-restore backup
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var preRestoreBackupPath = Path.Combine(
                Path.GetDirectoryName(currentDbPath)!,
                $"chronopos_preRestore_{timestamp}.db"
            );

            _logger.LogInformation("Creating pre-restore backup: {PreRestoreBackupPath}", preRestoreBackupPath);

            if (File.Exists(currentDbPath))
            {
                File.Copy(currentDbPath, preRestoreBackupPath, overwrite: true);
            }

            // Close any WAL files by deleting them (they'll be recreated)
            var walPath = currentDbPath + "-wal";
            var shmPath = currentDbPath + "-shm";
            
            try
            {
                if (File.Exists(walPath))
                {
                    _logger.LogInformation("Deleting WAL file: {WalPath}", walPath);
                    File.Delete(walPath);
                }
                if (File.Exists(shmPath))
                {
                    _logger.LogInformation("Deleting SHM file: {ShmPath}", shmPath);
                    File.Delete(shmPath);
                }
            }
            catch (Exception walEx)
            {
                _logger.LogWarning(walEx, "Could not delete WAL/SHM files - they may be locked");
            }

            // Apply restore using File.Replace for atomic operation
            _logger.LogInformation("Replacing database with restored backup");
            _logger.LogInformation("Source (staged): {StagedPath}, Destination: {CurrentDbPath}", stagedPath, currentDbPath);

            // File.Replace requires destination to exist
            if (!File.Exists(currentDbPath))
            {
                // If no current DB, just copy
                _logger.LogInformation("No existing database, copying staged backup");
                File.Copy(stagedPath, currentDbPath, overwrite: true);
            }
            else
            {
                // Use atomic replace - set backup parameter to null since we already made pre-restore backup
                _logger.LogInformation("Using File.Replace for atomic swap");
                File.Replace(stagedPath, currentDbPath, null);
            }
            
            _logger.LogInformation("Database file replaced successfully");

            // Verify the restored database
            try
            {
                int productCount = 0;
                int companyCount = 0;
                
                using var connection = new SqliteConnection($"Data Source={currentDbPath};Mode=ReadOnly");
                connection.Open();
                
                // Count products
                using var productCmd = connection.CreateCommand();
                productCmd.CommandText = "SELECT COUNT(*) FROM Products";
                productCount = Convert.ToInt32(productCmd.ExecuteScalar());
                
                // Count companies
                using var companyCmd = connection.CreateCommand();
                companyCmd.CommandText = "SELECT COUNT(*) FROM Companies";
                companyCount = Convert.ToInt32(companyCmd.ExecuteScalar());
                
                _logger.LogInformation("Restored database verification: {ProductCount} products, {CompanyCount} companies", 
                    productCount, companyCount);
            }
            catch (Exception verifyEx)
            {
                _logger.LogWarning(verifyEx, "Could not verify restored database contents");
            }

            // Clean up pending restore files
            try
            {
                File.Delete(metadataPath);
                if (File.Exists(stagedPath))
                {
                    File.Delete(stagedPath);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up pending restore files");
            }

            _logger.LogInformation("Restore applied successfully. Pre-restore backup saved at: {PreRestoreBackupPath}", preRestoreBackupPath);

            return new RestoreResult
            {
                Success = true,
                Message = "Database restored successfully from backup.",
                PreRestoreBackupPath = preRestoreBackupPath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying pending restore");
            return new RestoreResult
            {
                Success = false,
                Message = "Error applying restore.",
                ErrorDetails = ex.Message
            };
        }
    }

    /// <summary>
    /// Cancel pending restore
    /// </summary>
    public Task<bool> CancelPendingRestoreAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var metadataPath = GetPendingRestoreMetadataPath();
                var stagedPath = GetStagedBackupPath();

                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                if (File.Exists(stagedPath))
                {
                    File.Delete(stagedPath);
                }

                _logger.LogInformation("Pending restore cancelled successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending restore");
                return false;
            }
        });
    }

    /// <summary>
    /// Metadata for pending restore
    /// </summary>
    private class RestoreMetadata
    {
        public string OriginalBackupPath { get; set; } = string.Empty;
        public string StagedBackupPath { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public long BackupFileSize { get; set; }
        public int BackupTableCount { get; set; }
    }

    #endregion

    public void Dispose()
    {
        StopBackupScheduler();
    }
}
