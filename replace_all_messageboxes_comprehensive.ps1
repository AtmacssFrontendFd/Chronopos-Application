# Comprehensive PowerShell script to replace all remaining MessageBox.Show instances with custom dialogs
# Run this from the repository root directory

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "MessageBox Replacement Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Files to process - all remaining files with MessageBox
$filesToProcess = @(
    "src\ChronoPos.Desktop\ViewModels\ProductGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\AddStockTransferViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\DiscountViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductGroupsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\TaxTypesViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerGroupsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\PermissionViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductCombinationViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\AddOptionsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerManagementViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\RestaurantTableSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomersViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ManagementViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\PaymentTypesViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ReservationSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\App.xaml.cs",
    "src\ChronoPos.Desktop\Views\OnboardingWindow.xaml.cs",
    "src\ChronoPos.Desktop\Views\StockManagementView.xaml.cs",
    "src\ChronoPos.Desktop\Views\StartupOrchestrator.cs",
    "src\ChronoPos.Desktop\Views\LoginWindow.xaml.cs",
    "src\ChronoPos.Desktop\Views\Dialogs\MapPickerDialog.xaml.cs",
    "src\ChronoPos.Desktop\Views\CreateAdminWindow.xaml.cs"
)

$totalFiles = $filesToProcess.Count
$processedFiles = 0
$filesWithChanges = 0
$totalReplacements = 0

Write-Host "`nProcessing $totalFiles files...`n" -ForegroundColor Green

foreach ($file in $filesToProcess) {
    $processedFiles++
    
    if (-not (Test-Path $file)) {
        Write-Host "[$processedFiles/$totalFiles] ✗ File not found: $file" -ForegroundColor Red
        continue
    }

    Write-Host "[$processedFiles/$totalFiles] Processing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $file -Raw
    $originalContent = $content
    $fileReplacements = 0
    
    # Check if using statement exists
    $needsUsingStatement = $false
    if ($content -match 'MessageBox\.Show' -and $content -notmatch 'using ChronoPos\.Desktop\.Views\.Dialogs;') {
        $needsUsingStatement = $true
        Write-Host "  → Adding using statement..." -ForegroundColor Yellow
        
        # Find the last using statement and add after it
        if ($content -match '(?s)(using [^;]+;\s*)+') {
            $content = $content -replace '(using [^;]+;\s*)+', "`$&using ChronoPos.Desktop.Views.Dialogs;`n"
        }
    }
    
    # Pattern 1: Error messages with OK button
    if ($content -match 'MessageBox\.Show\([^,]+,\s*"[^"]+",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\)') {
        $pattern = 'MessageBox\.Show\(\s*\$?"([^"]*\{[^}]*\}[^"]*)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Error\s*\)'
        $replacement = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Error).ShowDialog()'
        $content = $content -replace $pattern, $replacement
        $matches = [regex]::Matches($originalContent, $pattern)
        $fileReplacements += $matches.Count
        
        # Handle non-interpolated strings
        $pattern2 = 'MessageBox\.Show\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Error\s*\)'
        $replacement2 = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Error).ShowDialog()'
        $content = $content -replace $pattern2, $replacement2
        $matches2 = [regex]::Matches($originalContent, $pattern2)
        $fileReplacements += $matches2.Count
    }
    
    # Pattern 2: Warning messages with OK button
    if ($content -match 'MessageBox\.Show\([^,]+,\s*"[^"]+",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\)') {
        $pattern = 'MessageBox\.Show\(\s*\$?"([^"]*\{[^}]*\}[^"]*)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Warning\s*\)'
        $replacement = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Warning).ShowDialog()'
        $content = $content -replace $pattern, $replacement
        $matches = [regex]::Matches($originalContent, $pattern)
        $fileReplacements += $matches.Count
        
        $pattern2 = 'MessageBox\.Show\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Warning\s*\)'
        $replacement2 = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Warning).ShowDialog()'
        $content = $content -replace $pattern2, $replacement2
        $matches2 = [regex]::Matches($originalContent, $pattern2)
        $fileReplacements += $matches2.Count
    }
    
    # Pattern 3: Information messages with OK button
    if ($content -match 'MessageBox\.Show\([^,]+,\s*"[^"]+",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\)') {
        $pattern = 'MessageBox\.Show\(\s*\$?"([^"]*\{[^}]*\}[^"]*)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Information\s*\)'
        $replacement = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Info).ShowDialog()'
        $content = $content -replace $pattern, $replacement
        $matches = [regex]::Matches($originalContent, $pattern)
        $fileReplacements += $matches.Count
        
        $pattern2 = 'MessageBox\.Show\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*MessageBoxButton\.OK\s*,\s*MessageBoxImage\.Information\s*\)'
        $replacement2 = 'new MessageDialog("$2", "$1", MessageDialog.MessageType.Info).ShowDialog()'
        $content = $content -replace $pattern2, $replacement2
        $matches2 = [regex]::Matches($originalContent, $pattern2)
        $fileReplacements += $matches2.Count
    }
    
    # Pattern 4: YesNo Question dialogs (multiline)
    $pattern = '(?s)var\s+result\s*=\s*MessageBox\.Show\(\s*\$?"([^"]*(?:\{[^}]*\})?[^"]*)",\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Question\s*\)'
    $replacement = 'var result = new ConfirmationDialog("$2", "$1", ConfirmationDialog.DialogType.Warning).ShowDialog()'
    if ($content -match $pattern) {
        $content = $content -replace $pattern, $replacement
        $matches = [regex]::Matches($originalContent, $pattern)
        $fileReplacements += $matches.Count
    }
    
    # Pattern 5: YesNo Warning dialogs (multiline)
    $pattern = '(?s)var\s+result\s*=\s*MessageBox\.Show\(\s*\$?"([^"]*(?:\{[^}]*\})?[^"]*)",\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Warning\s*\)'
    $replacement = 'var result = new ConfirmationDialog("$2", "$1", ConfirmationDialog.DialogType.Warning).ShowDialog()'
    if ($content -match $pattern) {
        $content = $content -replace $pattern, $replacement
        $matches = [regex]::Matches($originalContent, $pattern)
        $fileReplacements += $matches.Count
    }
    
    # Replace MessageBoxResult.Yes with true
    if ($content -match 'MessageBoxResult\.Yes') {
        $content = $content -replace 'result\s*==\s*MessageBoxResult\.Yes', 'result == true'
        $content = $content -replace 'result\s*!=\s*MessageBoxResult\.Yes', 'result != true'
        $content = $content -replace 'confirmResult\s*==\s*MessageBoxResult\.Yes', 'confirmResult == true'
        $content = $content -replace 'confirmResult\s*!=\s*MessageBoxResult\.Yes', 'confirmResult != true'
    }
    
    # Check if any changes were made
    if ($content -ne $originalContent) {
        Set-Content $file -Value $content -NoNewline
        $filesWithChanges++
        $totalReplacements += $fileReplacements
        Write-Host "  ✓ Replaced $fileReplacements instance(s)" -ForegroundColor Green
    }
    else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Replacement Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Total files processed: $processedFiles" -ForegroundColor White
Write-Host "Files modified: $filesWithChanges" -ForegroundColor Green
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Green
Write-Host "`n✓ MessageBox replacement complete!" -ForegroundColor Green

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Review changes: git diff" -ForegroundColor White
Write-Host "2. Build solution: dotnet build" -ForegroundColor White
Write-Host "3. Test the application" -ForegroundColor White
Write-Host "4. Commit changes: git add . && git commit -m 'Replace MessageBox with custom dialogs'" -ForegroundColor White
