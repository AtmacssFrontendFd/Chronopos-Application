# Comprehensive MessageBox Replacement Script
# This script replaces all MessageBox.Show calls with custom dialog boxes across the entire solution

Write-Host "================================" -ForegroundColor Cyan
Write-Host "MessageBox Replacement Tool" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Define the root directory
$rootDir = $PSScriptRoot
$srcDir = Join-Path $rootDir "src"

# Files to process (ViewModels and Views)
$filesToProcess = Get-ChildItem -Path $srcDir -Recurse -Include "*.cs" | 
    Where-Object { 
        $_.FullName -notlike "*\obj\*" -and 
        $_.FullName -notlike "*\bin\*" -and
        ($_.Name -like "*ViewModel.cs" -or $_.Name -like "*.xaml.cs")
    }

Write-Host "Found $($filesToProcess.Count) files to process" -ForegroundColor Yellow
Write-Host ""

$totalReplacements = 0
$filesModified = 0

foreach ($file in $filesToProcess) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor White
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileReplacements = 0
    
    # Check if file needs the using statement
    $needsUsing = $content -match "MessageBox\.Show" -and $content -notmatch "using ChronoPos\.Desktop\.Views\.Dialogs;"
    
    if ($needsUsing) {
        # Find the last using statement and add our using after it
        if ($content -match "(?s)(using [^;]+;)(\s*namespace)") {
            $lastUsing = $matches[1]
            $content = $content -replace "(?s)(using [^;]+;)(\s*namespace)", "`$1`nusing ChronoPos.Desktop.Views.Dialogs;`$2"
            Write-Host "  ✓ Added using statement" -ForegroundColor Green
        }
    }
    
    # Pattern 1: Simple Error Messages (single line)
    # MessageBox.Show("message", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
    $pattern1 = 'MessageBox\.Show\(\`$?"([^"]+)",\s*"Error",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)'
    $replacement1 = 'new MessageDialog("Error", `$"$1", MessageDialog.MessageType.Error).ShowDialog()'
    if ($content -match $pattern1) {
        $content = $content -replace $pattern1, $replacement1
        $count = ([regex]::Matches($originalContent, $pattern1)).Count
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count error message(s)" -ForegroundColor Green
    }
    
    # Pattern 2: Warning Messages (single line)
    # MessageBox.Show("message", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
    $pattern2 = 'MessageBox\.Show\(\`$?"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\)'
    $replacement2 = 'new MessageDialog("$2", `$"$1", MessageDialog.MessageType.Warning).ShowDialog()'
    if ($content -match $pattern2) {
        $beforeCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $content = $content -replace $pattern2, $replacement2
        $afterCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $count = $beforeCount - $afterCount
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count warning message(s)" -ForegroundColor Green
    }
    
    # Pattern 3: Info Messages (single line)
    # MessageBox.Show("message", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
    $pattern3 = 'MessageBox\.Show\(\`$?"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\)'
    $replacement3 = 'new MessageDialog("$2", `$"$1", MessageDialog.MessageType.Info).ShowDialog()'
    if ($content -match $pattern3) {
        $beforeCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $content = $content -replace $pattern3, $replacement3
        $afterCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $count = $beforeCount - $afterCount
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count info message(s)" -ForegroundColor Green
    }
    
    # Pattern 4: Multi-line permission denied messages
    # MessageBox.Show(
    #     "message",
    #     "Access Denied",
    #     MessageBoxButton.OK,
    #     MessageBoxImage.Warning);
    $pattern4 = 'MessageBox\.Show\(\s*"([^"]+)",\s*"Access Denied",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\)'
    $replacement4 = 'new MessageDialog(
                "Access Denied",
                "$1",
                MessageDialog.MessageType.Warning).ShowDialog()'
    if ($content -match $pattern4) {
        $beforeCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $content = $content -replace $pattern4, $replacement4
        $afterCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $count = $beforeCount - $afterCount
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count access denied message(s)" -ForegroundColor Green
    }
    
    # Pattern 5: Yes/No Confirmation dialogs
    # var result = MessageBox.Show("message", "title", MessageBoxButton.YesNo, MessageBoxImage.Question);
    # if (result == MessageBoxResult.Yes)
    $pattern5 = 'var result = MessageBox\.Show\(\s*\`$?"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.(Question|Warning)\);'
    $replacement5 = 'var dialog = new ConfirmationDialog(
            "$2",
            `$"$1",
            ConfirmationDialog.DialogType.Warning);
        
        var result = dialog.ShowDialog();'
    if ($content -match $pattern5) {
        $beforeCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $content = $content -replace $pattern5, $replacement5
        $afterCount = ([regex]::Matches($content, 'MessageBox\.Show')).Count
        $count = $beforeCount - $afterCount
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count confirmation dialog(s)" -ForegroundColor Green
    }
    
    # Pattern 6: Replace MessageBoxResult.Yes with true
    if ($content -match 'MessageBoxResult\.Yes') {
        $content = $content -replace 'if \(result == MessageBoxResult\.Yes\)', 'if (result == true)'
        Write-Host "  ✓ Replaced MessageBoxResult.Yes with true" -ForegroundColor Green
    }
    
    # Pattern 7: System.Windows.MessageBox.Show (fully qualified)
    $pattern7 = 'System\.Windows\.MessageBox\.Show\(\`$?"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)'
    $replacement7 = 'new MessageDialog("$2", `$"$1", MessageDialog.MessageType.Error).ShowDialog()'
    if ($content -match $pattern7) {
        $beforeCount = ([regex]::Matches($content, 'System\.Windows\.MessageBox\.Show')).Count
        $content = $content -replace $pattern7, $replacement7
        $afterCount = ([regex]::Matches($content, 'System\.Windows\.MessageBox\.Show')).Count
        $count = $beforeCount - $afterCount
        $fileReplacements += $count
        Write-Host "  ✓ Replaced $count System.Windows.MessageBox error(s)" -ForegroundColor Green
    }
    
    # Pattern 8: Multi-line error messages with variables
    # MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    # Already covered by pattern 1, but let's handle edge cases
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $filesModified++
        $totalReplacements += $fileReplacements
        Write-Host "  ✅ Modified file ($fileReplacements replacements)" -ForegroundColor Cyan
        Write-Host ""
    } else {
        # Check if file still has MessageBox.Show
        if ($content -match "MessageBox\.Show") {
            Write-Host "  ⚠ File still contains MessageBox.Show (complex patterns - needs manual review)" -ForegroundColor Yellow
            # Show the first few MessageBox.Show occurrences
            $matches = [regex]::Matches($content, "MessageBox\.Show[^\n]{0,100}")
            if ($matches.Count -gt 0) {
                Write-Host "  Sample: $($matches[0].Value)..." -ForegroundColor DarkYellow
            }
            Write-Host ""
        } else {
            Write-Host "  ✓ No MessageBox found" -ForegroundColor DarkGray
        }
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Files Modified: $filesModified" -ForegroundColor Green
Write-Host "Total Replacements: $totalReplacements" -ForegroundColor Green
Write-Host ""

# Check for remaining MessageBox.Show occurrences
Write-Host "Checking for remaining MessageBox instances..." -ForegroundColor Yellow
$remainingFiles = Get-ChildItem -Path $srcDir -Recurse -Include "*.cs" | 
    Where-Object { 
        $_.FullName -notlike "*\obj\*" -and 
        $_.FullName -notlike "*\bin\*"
    } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content -match "MessageBox\.Show") {
            [PSCustomObject]@{
                File = $_.Name
                Path = $_.FullName
                Count = ([regex]::Matches($content, "MessageBox\.Show")).Count
            }
        }
    }

if ($remainingFiles) {
    Write-Host ""
    Write-Host "⚠ Files still containing MessageBox.Show:" -ForegroundColor Yellow
    $remainingFiles | ForEach-Object {
        Write-Host "  - $($_.File): $($_.Count) occurrence(s)" -ForegroundColor Yellow
        Write-Host "    $($_.Path)" -ForegroundColor DarkGray
    }
    Write-Host ""
    Write-Host "These files may contain complex patterns that need manual review." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "✅ No remaining MessageBox.Show instances found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done! Press any key to exit..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
