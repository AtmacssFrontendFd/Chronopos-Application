# Database Restore Implementation - Complete Guide

## Overview
This document describes the complete implementation of the database restore functionality for ChronoPos. The feature allows users to restore their database from a backup file using a safe, staged approach.

## Implementation Summary

### ✅ Backend Implementation (COMPLETE)

#### 1. Interface Definition (`IBackupService.cs`)
Added the following methods to the backup service interface:

```csharp
Task<RestoreResult> StageRestoreAsync(string backupFilePath);
Task<bool> HasPendingRestoreAsync();
Task<RestoreResult> ApplyPendingRestoreAsync();
Task<bool> CancelPendingRestoreAsync();
Task<BackupValidationResult> ValidateBackupFileAsync(string backupFilePath);
```

**Result Classes:**
- `RestoreResult`: Contains Success, Message, PreRestoreBackupPath, ErrorDetails
- `BackupValidationResult`: Contains IsValid, Message, TableCount, FileSize

#### 2. Service Implementation (`BackupService.cs`)

**Key Features:**
- **Schema Validation**: Uses `Microsoft.Data.Sqlite` to open backup files read-only and validate structure
- **Staged Approach**: Copies backup to pending area (`%LOCALAPPDATA%\ChronoPos\PendingRestore\`)
- **Metadata Tracking**: JSON file (`restore_pending.json`) tracks restore request details
- **Atomic Operations**: Uses `File.Replace()` for transactional database swap
- **Pre-Restore Backup**: Creates timestamped backup before applying restore (e.g., `chronopos_preRestore_20231215_143022.db`)

**Validation Checks:**
- File exists and is readable
- Valid SQLite database (checks file header)
- Contains tables (table count > 0)
- Minimum file size (> 1KB)

**Private Helper Methods:**
- `GetPendingRestoreDirectory()`
- `GetPendingRestoreMetadataPath()`
- `GetStagedBackupPath()`

**Metadata Structure:**
```json
{
  "OriginalBackupPath": "C:\\path\\to\\backup.db",
  "StagedBackupPath": "%LOCALAPPDATA%\\ChronoPos\\PendingRestore\\backup_staged.db",
  "RequestedAt": "2023-12-15T14:30:22Z",
  "BackupFileSize": 1048576,
  "BackupTableCount": 45
}
```

#### 3. Startup Integration (`App.xaml.cs`)

Added `CheckAndApplyPendingRestore()` method (lines 1017-1089) that:
1. Creates DI scope and gets `IBackupService`
2. Calls `HasPendingRestoreAsync()` to check for staged restore
3. If pending, calls `ApplyPendingRestoreAsync()` and processes result
4. Shows success dialog with pre-restore backup location
5. Shows error dialog if restore fails
6. Logs all operations with `AppLogger` (filename: "restore")
7. Non-blocking: Shows warning but doesn't prevent app startup if restore check fails

**Integration Point:**
```csharp
// Line 695 in OnStartup (BEFORE InitializeDatabase)
CheckAndApplyPendingRestore();
await InitializeDatabase();
```

### ✅ Frontend Implementation (COMPLETE)

#### 4. ViewModel (`CompanySettingsViewModel.cs`)

**Properties:**
- `RestoreBackupPath` (string?): Path to selected backup file
- `HasPendingRestore` (bool): Indicates if restore is pending

**Commands:**
- `BrowseRestoreBackupPathCommand`: Opens OpenFileDialog with `.db` filter
- `StageRestoreCommand`: Validates and stages restore with confirmation dialog
- `CancelPendingRestoreCommand`: Cancels pending restore

**Initialization:**
- `CheckPendingRestoreAsync()` called in `InitializeAsync()` to set `HasPendingRestore`

**Error Handling:**
- Validates backup path before staging
- Shows confirmation dialog with warning about restart requirement
- Displays success/failure messages with details
- Logs all operations

#### 5. UI (`CompanySettingsUserControl.xaml`)

**Database Restore Section** (added after Backup Settings):

1. **Warning Notice** (Yellow Box):
   - Warns about database replacement
   - Lists safety features (auto-backup, restart requirement, version compatibility)

2. **Browse Backup Control**:
   - TextBox showing selected backup path (read-only)
   - "📁 Browse Backup" button (Primary color)

3. **Stage Restore Button**:
   - Red button with warning emoji "⚠️ Stage Restore (Requires Restart)"
   - Prominent styling to indicate critical action

4. **Pending Restore Status** (Green Box):
   - Visible only when `HasPendingRestore` is true
   - Shows "✅ Restore Pending" message
   - Explains restore will apply on restart
   - "❌ Cancel Pending Restore" button (Yellow/Warning color)

## Safety Features

### 1. **Staged Restore (No Race Conditions)**
- Restore is staged to pending area first
- Applied BEFORE any EF/DbContext initialization
- No open database connections during restore operation
- Eliminates file lock issues completely

### 2. **Atomic File Operations**
- Uses `File.Replace(source, dest, backup)` for transactional swap
- All-or-nothing operation (no partial failures)
- Original file preserved as fallback

### 3. **Pre-Restore Backups**
- Automatic timestamped backup before restore
- Format: `chronopos_preRestore_yyyyMMdd_HHmmss.db`
- Location shown in success dialog for easy rollback

### 4. **Schema Validation**
- Opens backup file read-only to validate structure
- Checks table count and file size
- Prevents incompatible database restores
- Catches `SqliteException` for invalid files

### 5. **User Confirmation**
- Clear warning dialogs before staging restore
- Explains restart requirement
- Shows backup file name
- Mentions automatic pre-restore backup

### 6. **Comprehensive Logging**
- All operations logged with `AppLogger` (filename: "restore")
- Tracks: staging, validation, apply, cancel operations
- Includes file paths, timestamps, error details

### 7. **Non-Blocking Error Handling**
- Restore check failures don't prevent app startup
- Shows warning dialog but allows normal operation
- User can try restore again later

## User Workflow

### Staging a Restore:
1. User opens **Settings** → **Company Settings**
2. Scrolls to **Database Restore** section
3. Clicks **"📁 Browse Backup"** button
4. Selects backup `.db` file from file dialog
5. Reviews warning notice about database replacement
6. Clicks **"⚠️ Stage Restore (Requires Restart)"** button
7. Confirms action in confirmation dialog
8. Sees success message: "✅ Restore staged successfully! Please restart the application."
9. **Pending Restore Status** box appears (green)

### Applying the Restore:
1. User closes and restarts ChronoPos application
2. On startup, `CheckAndApplyPendingRestore()` runs automatically
3. Creates pre-restore backup (e.g., `chronopos_preRestore_20231215_143022.db`)
4. Applies staged restore using atomic `File.Replace()`
5. Shows success dialog with pre-restore backup location
6. User can continue using restored database

### Canceling a Pending Restore:
1. User opens **Settings** → **Company Settings**
2. Sees **"✅ Restore Pending"** status box (green)
3. Clicks **"❌ Cancel Pending Restore"** button
4. Confirms cancellation in dialog
5. Pending restore removed, can stage different backup

### Rolling Back (if needed):
1. User navigates to `%LOCALAPPDATA%\ChronoPos\`
2. Finds timestamped pre-restore backup (shown in success dialog)
3. Copies it to replace `chronopos.db`
4. Restarts application

## File Locations

- **Live Database**: `%LOCALAPPDATA%\ChronoPos\chronopos.db`
- **Pending Restore Directory**: `%LOCALAPPDATA%\ChronoPos\PendingRestore\`
- **Staged Backup**: `%LOCALAPPDATA%\ChronoPos\PendingRestore\backup_staged.db`
- **Metadata File**: `%LOCALAPPDATA%\ChronoPos\PendingRestore\restore_pending.json`
- **Pre-Restore Backups**: `%LOCALAPPDATA%\ChronoPos\chronopos_preRestore_*.db`

## Testing Plan

### Manual Testing:

1. **Valid Backup Restore:**
   - Create backup using existing backup feature
   - Browse and select backup file
   - Stage restore (verify pending status appears)
   - Restart application
   - Verify restore success dialog appears
   - Verify pre-restore backup file exists
   - Check that data matches backup

2. **Invalid Backup File:**
   - Try to stage a non-SQLite file
   - Verify validation error appears
   - Verify restore is NOT staged

3. **Missing Backup File:**
   - Select backup, then delete it before staging
   - Verify "file not found" error

4. **Cancel Pending Restore:**
   - Stage a restore
   - Cancel it before restarting
   - Restart application
   - Verify NO restore dialog appears
   - Verify database unchanged

5. **Rollback Test:**
   - Make note of current data
   - Stage and apply restore
   - Note pre-restore backup path from success dialog
   - Manually copy pre-restore backup over live database
   - Restart application
   - Verify original data is restored

### Edge Cases:

- **File Permissions**: Test with read-only backup file
- **Disk Space**: Test with insufficient disk space
- **Concurrent Access**: Ensure no other process has DB file open
- **Version Compatibility**: Test with backups from different app versions
- **Large Backups**: Test with large database files (> 100MB)

## Dependencies

- **Microsoft.Data.Sqlite**: For backup validation (read-only connections)
- **System.Text.Json**: For metadata serialization
- **Microsoft.Win32**: For OpenFileDialog

## Logging

All restore operations are logged to: `logs/restore_YYYYMMDD.log`

**Log Entries Include:**
- Restore staging requests with file paths
- Validation results (table count, file size)
- Pending restore checks
- Restore application attempts
- Pre-restore backup creation
- Success/failure messages
- Exception details

## Security Considerations

1. **File Validation**: All backup files are validated before staging
2. **Read-Only Access**: Backup files opened read-only during validation
3. **No User Input Execution**: Backup paths validated, no code execution
4. **Atomic Operations**: No partial database states possible
5. **Rollback Capability**: Pre-restore backups enable recovery

## Limitations

1. **Requires Restart**: Cannot apply restore while application is running
2. **Offline Only**: Feature designed for offline use (no network backup sources)
3. **Version Compatibility**: Schema changes between versions may cause issues
4. **Single Pending Restore**: Only one restore can be pending at a time

## Future Enhancements

- [ ] Automatic version compatibility check
- [ ] Restore preview (show backup metadata before staging)
- [ ] Restore history tracking
- [ ] Multiple restore points management
- [ ] Scheduled automatic restores
- [ ] Network backup source support
- [ ] Incremental restore capability

## Status

✅ **FULLY IMPLEMENTED AND READY FOR TESTING**

All backend and frontend components are complete:
- ✅ IBackupService interface extended
- ✅ BackupService implementation
- ✅ App.xaml.cs startup integration
- ✅ CompanySettingsViewModel commands
- ✅ CompanySettingsUserControl.xaml UI
- ✅ Validation, logging, error handling
- ✅ Pre-restore backups
- ✅ User dialogs and feedback

Next step: Build and test the implementation.

## Support

For issues or questions about the database restore feature:
1. Check logs in `logs/restore_YYYYMMDD.log`
2. Verify pre-restore backup exists in `%LOCALAPPDATA%\ChronoPos\`
3. Review validation error messages
4. Check file permissions on backup files

---

*Implementation Date: December 2024*
*Feature: Staged Database Restore*
*Status: Complete - Ready for Testing*
