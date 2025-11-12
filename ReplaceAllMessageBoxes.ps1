# PowerShell script to replace all MessageBox.Show with custom dialogs
# This script will update all C# files in the ChronoPos.Desktop project

$projectPath = "c:\Users\saswa\OneDrive\Desktop\chronoPOS\ChronoPosRevised\src\ChronoPos.Desktop"
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }

$addedUsings = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Check if file has any MessageBox.Show calls
    if ($content -match "MessageBox\.Show|System\.Windows\.MessageBox\.Show") {
        Write-Host "Processing: $($file.FullName)" -ForegroundColor Yellow
        
        # Add using statement if not already present
        if ($content -notmatch "using ChronoPos\.Desktop\.Views\.Dialogs;") {
            # Find the last using statement
            if ($content -match "(?s)(using [^;]+;)(\r?\n)*namespace") {
                $lastUsing = $matches[1]
                $content = $content -replace "($([regex]::Escape($lastUsing)))(\r?\n)+(namespace)", "`$1`r`nusing ChronoPos.Desktop.Views.Dialogs;`r`n`r`n`$3"
                $modified = $true
                Write-Host "  Added using statement" -ForegroundColor Green
            }
        }
        
        # Replace simple error messages: MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        $pattern1 = 'MessageBox\.Show\(([^,]+),\s*"Error"[^,]*,\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)'
        if ($content -match $pattern1) {
            $content = $content -replace $pattern1, 'new MessageDialog("Error", $1, MessageDialog.MessageType.Error).ShowDialog()'
            $modified = $true
            Write-Host "  Replaced Error MessageBox" -ForegroundColor Green
        }
        
        # Replace System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        $pattern1b = 'System\.Windows\.MessageBox\.Show\(([^,]+),\s*"Error"[^,]*,\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)'
        if ($content -match $pattern1b) {
            $content = $content -replace $pattern1b, 'new MessageDialog("Error", $1, MessageDialog.MessageType.Error).ShowDialog()'
            $modified = $true
            Write-Host "  Replaced System.Windows Error MessageBox" -ForegroundColor Green
        }
        
        # Replace warning messages
        $pattern2 = 'MessageBox\.Show\(([^,]+),\s*"([^"]+)"[^,]*,\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\)'
        if ($content -match $pattern2) {
            $content = $content -replace $pattern2, 'new MessageDialog("$2", $1, MessageDialog.MessageType.Warning).ShowDialog()'
            $modified = $true
            Write-Host "  Replaced Warning MessageBox" -ForegroundColor Green
        }
        
        # Replace success messages  
        $pattern3 = 'MessageBox\.Show\(([^,]+),\s*"Success"[^,]*,\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\)'
        if ($content -match $pattern3) {
            $content = $content -replace $pattern3, 'new MessageDialog("Success", $1, MessageDialog.MessageType.Success).ShowDialog()'
            $modified = $true
            Write-Host "  Replaced Success MessageBox" -ForegroundColor Green
        }
        
        # Replace info messages
        $pattern4 = 'MessageBox\.Show\(([^,]+),\s*"([^"]+)"[^,]*,\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\)'
        if ($content -match $pattern4) {
            $content = $content -replace $pattern4, 'new MessageDialog("$2", $1, MessageDialog.MessageType.Info).ShowDialog()'
            $modified = $true
            Write-Host "  Replaced Info MessageBox" -ForegroundColor Green
        }
    }
    
    # Save if modified
    if ($modified -and $content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $addedUsings += $file.FullName
        Write-Host "  SAVED: $($file.Name)" -ForegroundColor Cyan
    }
}

Write-Host "`nProcessing complete!" -ForegroundColor Green
Write-Host "Modified $($addedUsings.Count) files" -ForegroundColor Green
