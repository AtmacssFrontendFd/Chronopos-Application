# Simple MessageBox Replacement Script
# This script replaces MessageBox.Show calls with custom dialogs

Write-Host "================================" -ForegroundColor Cyan
Write-Host "MessageBox Replacement Tool" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$rootDir = $PSScriptRoot
$srcDir = Join-Path $rootDir "src"

# Find all CS files in ViewModels and Views
$files = Get-ChildItem -Path $srcDir -Recurse -Include "*.cs" | 
    Where-Object { 
        $_.FullName -notlike "*\obj\*" -and 
        $_.FullName -notlike "*\bin\*"
    }

Write-Host "Found $($files.Count) files to check" -ForegroundColor Yellow
Write-Host ""

$totalFiles = 0
$totalReplacements = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $originalContent = $content
    $changed = $false
    
    # Only process files with MessageBox.Show
    if (-not ($content -match "MessageBox\.Show")) {
        continue
    }
    
    Write-Host "Processing: $($file.Name)" -ForegroundColor White
    $fileReplacements = 0
    
    # Add using statement if needed
    if ($content -notmatch "using ChronoPos\.Desktop\.Views\.Dialogs;") {
        # Find position after last using statement
        if ($content -match "(?s)(using [^\r\n]+;)(\r?\n)(namespace)") {
            $content = $content -replace "(using [^\r\n]+;)(\r?\n)(namespace)", "`$1`r`nusing ChronoPos.Desktop.Views.Dialogs;`$2`$3"
            Write-Host "  + Added using statement" -ForegroundColor Green
            $changed = $true
        }
    }
    
    # Count before
    $beforeCount = ([regex]::Matches($content, "MessageBox\.Show")).Count
    
    # Replace single-line error messages
    $content = $content -replace 'MessageBox\.Show\(([^,]+),\s*"Error",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)', 'new MessageDialog("Error", $1, MessageDialog.MessageType.Error).ShowDialog()'
    
    # Replace single-line warning messages  
    $content = $content -replace 'MessageBox\.Show\(([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Warning).ShowDialog()'
    
    # Replace multi-line simple patterns
    $content = $content -replace '(?s)MessageBox\.Show\(\s*([^,]+?),\s*"Error",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)', 'new MessageDialog("Error", $1, MessageDialog.MessageType.Error).ShowDialog()'
    
    $content = $content -replace '(?s)MessageBox\.Show\(\s*([^,]+?),\s*"([^"]+?)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Warning).ShowDialog()'
    
    # Replace System.Windows.MessageBox.Show
    $content = $content -replace 'System\.Windows\.MessageBox\.Show\(([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Error).ShowDialog()'
    
    # Replace MessageBoxResult.Yes with true
    $content = $content -replace 'if \(result == MessageBoxResult\.Yes\)', 'if (result == true)'
    
    # Count after
    $afterCount = ([regex]::Matches($content, "MessageBox\.Show")).Count
    $fileReplacements = $beforeCount - $afterCount
    
    if ($content -ne $originalContent) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $totalFiles++
        $totalReplacements += $fileReplacements
        Write-Host "  ✓ Made $fileReplacements replacement(s)" -ForegroundColor Green
        
        if ($afterCount -gt 0) {
            Write-Host "  ⚠ Still has $afterCount MessageBox instances (complex patterns)" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
}

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Files Modified: $totalFiles" -ForegroundColor Green
Write-Host "Total Replacements: $totalReplacements" -ForegroundColor Green
Write-Host ""

# Final check
Write-Host "Checking for remaining MessageBox instances..." -ForegroundColor Yellow
$remaining = Get-ChildItem -Path $srcDir -Recurse -Include "*.cs" | 
    Where-Object { 
        $_.FullName -notlike "*\obj\*" -and 
        $_.FullName -notlike "*\bin\*"
    } |
    ForEach-Object {
        $content = [System.IO.File]::ReadAllText($_.FullName)
        $count = ([regex]::Matches($content, "MessageBox\.Show")).Count
        if ($count -gt 0) {
            [PSCustomObject]@{
                File = $_.Name
                Count = $count
            }
        }
    }

if ($remaining) {
    Write-Host ""
    $remaining | ForEach-Object {
        Write-Host "  - $($_.File): $($_.Count) remaining" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Total remaining: $(($remaining | Measure-Object -Property Count -Sum).Sum)" -ForegroundColor Yellow
} else {
    Write-Host "All MessageBox instances replaced!" -ForegroundColor Green
}

Write-Host ""
