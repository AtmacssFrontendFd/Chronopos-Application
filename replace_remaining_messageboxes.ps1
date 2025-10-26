# PowerShell script to replace remaining MessageBox.Show instances with custom dialogs
# Run this from the repository root directory

$ErrorActionPreference = "Stop"

# Define replacement patterns
$replacements = @(
    # MessageBox.Show with OK button - to MessageDialog
    @{
        Pattern = 'MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)'
        Replacement = 'new MessageDialog("$2", $1, MessageDialog.MessageType.Error).ShowDialog()'
    },
    @{
        Pattern = 'MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\s*\)'
        Replacement = 'new MessageDialog("$2", $1, MessageDialog.MessageType.Warning).ShowDialog()'
    },
    @{
        Pattern = 'MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\s*\)'
        Replacement = 'new MessageDialog("$2", $1, MessageDialog.MessageType.Info).ShowDialog()'
    },
    @{
        Pattern = 'MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Success\s*\)'
        Replacement = 'new MessageDialog("$2", $1, MessageDialog.MessageType.Success).ShowDialog()'
    },
    # MessageBox.Show with YesNo button - to ConfirmationDialog
    @{
        Pattern = 'var\s+result\s*=\s*MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Question\s*\)'
        Replacement = 'var result = new ConfirmationDialog("$2", $1, ConfirmationDialog.DialogType.Warning).ShowDialog()'
    },
    @{
        Pattern = 'var\s+result\s*=\s*MessageBox\.Show\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Warning\s*\)'
        Replacement = 'var result = new ConfirmationDialog("$2", $1, ConfirmationDialog.DialogType.Warning).ShowDialog()'
    },
    # MessageBoxResult.Yes to result == true
    @{
        Pattern = 'result\s*==\s*MessageBoxResult\.Yes'
        Replacement = 'result == true'
    },
    @{
        Pattern = 'result\s*!=\s*MessageBoxResult\.Yes'
        Replacement = 'result != true'
    }
)

# Files to process (add all remaining files with MessageBox)
$filesToProcess = @(
    "src\ChronoPos.Desktop\ViewModels\ReservationTimelineViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\TaxTypesViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductGroupsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductCombinationViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\PermissionViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ReservationSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\RestaurantTableSidePanelViewModel.cs"
)

Write-Host "Starting MessageBox replacement..." -ForegroundColor Green

foreach ($file in $filesToProcess) {
    if (Test-Path $file) {
        Write-Host "`nProcessing: $file" -ForegroundColor Cyan
        
        # Check if using statement exists
        $content = Get-Content $file -Raw
        if ($content -notmatch 'using ChronoPos\.Desktop\.Views\.Dialogs;') {
            Write-Host "  Adding using statement..." -ForegroundColor Yellow
            $content = $content -replace '(using [^;]+;\s*)+', "`$&using ChronoPos.Desktop.Views.Dialogs;`n"
            Set-Content $file -Value $content -NoNewline
        }
        
        # Apply replacements
        $content = Get-Content $file -Raw
        $changesMade = $false
        
        foreach ($replacement in $replacements) {
            if ($content -match $replacement.Pattern) {
                $content = $content -replace $replacement.Pattern, $replacement.Replacement
                $changesMade = $true
            }
        }
        
        if ($changesMade) {
            Set-Content $file -Value $content -NoNewline
            Write-Host "  ✓ Replacements applied" -ForegroundColor Green
        } else {
            Write-Host "  - No changes needed" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ✗ File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`n✓ MessageBox replacement complete!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Review the changes with: git diff"
Write-Host "2. Build the solution: dotnet build"
Write-Host "3. Test the application"
