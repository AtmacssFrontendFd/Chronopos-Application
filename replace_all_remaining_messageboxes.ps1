# PowerShell script to replace all remaining MessageBox.Show instances with custom dialogs
# Run this from the repository root directory

$ErrorActionPreference = "Stop"

Write-Host "Starting comprehensive MessageBox replacement..." -ForegroundColor Green

# Define the files that need to be updated
$filesToProcess = @(
    # ViewModels
    "src\ChronoPos.Desktop\ViewModels\AddOptionsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\AddStockTransferViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerGroupSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerGroupsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomerManagementViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\CustomersViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\DiscountViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ManagementViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\PaymentTypesViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\PermissionViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductCombinationViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductGroupsViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ProductModifierViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\ReservationSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\RestaurantTableSidePanelViewModel.cs",
    "src\ChronoPos.Desktop\ViewModels\TaxTypesViewModel.cs",
    
    # Views and other files
    "src\ChronoPos.Desktop\App.xaml.cs",
    "src\ChronoPos.Desktop\Views\CreateAdminWindow.xaml.cs",
    "src\ChronoPos.Desktop\Views\Dialogs\MapPickerDialog.xaml.cs",
    "src\ChronoPos.Desktop\Views\LoginWindow.xaml.cs",
    "src\ChronoPos.Desktop\Views\OnboardingWindow.xaml.cs",
    "src\ChronoPos.Desktop\Views\StartupOrchestrator.cs",
    "src\ChronoPos.Desktop\Views\StockManagementView.xaml.cs"
)

foreach ($file in $filesToProcess) {
    if (Test-Path $file) {
        Write-Host "`nProcessing: $file" -ForegroundColor Cyan
        
        $content = Get-Content $file -Raw
        $originalContent = $content
        $changesMade = $false
        
        # Check if using statement exists
        if ($content -notmatch 'using ChronoPos\.Desktop\.Views\.Dialogs;') {
            Write-Host "  Adding using statement..." -ForegroundColor Yellow
            $content = $content -replace '(using [^;]+;\s*)+', "`$&using ChronoPos.Desktop.Views.Dialogs;`n"
            $changesMade = $true
        }
        
        # Replace MessageBox.Show with OK button - to MessageDialog
        if ($content -match 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Error).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (OK, Error)" -ForegroundColor Green
        }
        
        if ($content -match 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Warning\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Warning).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (OK, Warning)" -ForegroundColor Green
        }
        
        if ($content -match 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Info).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (OK, Information)" -ForegroundColor Green
        }
        
        if ($content -match 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Success\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Success\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Success).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (OK, Success)" -ForegroundColor Green
        }
        
        # Replace simple MessageBox.Show calls without explicit button/image
        if ($content -match 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)"\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)"\s*\)', 'new MessageDialog("$2", $1, MessageDialog.MessageType.Info).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced simple MessageBox.Show" -ForegroundColor Green
        }
        
        # Replace MessageBox.Show with YesNo button - to ConfirmationDialog
        if ($content -match 'var\s+result\s*=\s*MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Question\s*\)') {
            $content = $content -replace 'var\s+result\s*=\s*MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Question\s*\)', 'var result = new ConfirmationDialog("$2", $1, ConfirmationDialog.DialogType.Warning).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (YesNo, Question)" -ForegroundColor Green
        }
        
        if ($content -match 'var\s+result\s*=\s*MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Warning\s*\)') {
            $content = $content -replace 'var\s+result\s*=\s*MessageBox\.Show\s*\(\s*([^,]+),\s*"([^"]+)",\s*MessageBoxButton\.YesNo,\s*MessageBoxImage\.Warning\s*\)', 'var result = new ConfirmationDialog("$2", $1, ConfirmationDialog.DialogType.Warning).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show (YesNo, Warning)" -ForegroundColor Green
        }
        
        # Replace MessageBoxResult.Yes to result == true
        if ($content -match 'result\s*==\s*MessageBoxResult\.Yes') {
            $content = $content -replace 'result\s*==\s*MessageBoxResult\.Yes', 'result == true'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBoxResult.Yes" -ForegroundColor Green
        }
        
        if ($content -match 'result\s*!=\s*MessageBoxResult\.Yes') {
            $content = $content -replace 'result\s*!=\s*MessageBoxResult\.Yes', 'result != true'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBoxResult.Yes (!=)" -ForegroundColor Green
        }
        
        # Handle more complex patterns
        # Pattern: MessageBox.Show($"...", "Title", MessageBoxButton.OK, MessageBoxImage.Error);
        if ($content -match 'MessageBox\.Show\s*\(\s*\$"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*\$"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Error\s*\)', 'new MessageDialog("$2", $"$1", MessageDialog.MessageType.Error).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show with string interpolation (Error)" -ForegroundColor Green
        }
        
        if ($content -match 'MessageBox\.Show\s*\(\s*\$"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\s*\)') {
            $content = $content -replace 'MessageBox\.Show\s*\(\s*\$"([^"]+)",\s*"([^"]+)",\s*MessageBoxButton\.OK,\s*MessageBoxImage\.Information\s*\)', 'new MessageDialog("$2", $"$1", MessageDialog.MessageType.Info).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced MessageBox.Show with string interpolation (Info)" -ForegroundColor Green
        }
        
        # Handle System.Windows.MessageBox.Show
        if ($content -match 'System\.Windows\.MessageBox\.Show\s*\(\s*([^)]+)\)') {
            $content = $content -replace 'System\.Windows\.MessageBox\.Show\s*\(\s*([^)]+)\)', 'new MessageDialog("Info", $1, MessageDialog.MessageType.Info).ShowDialog()'
            $changesMade = $true
            Write-Host "  ✓ Replaced System.Windows.MessageBox.Show" -ForegroundColor Green
        }
        
        if ($changesMade) {
            Set-Content $file -Value $content -NoNewline
            Write-Host "  ✓ File updated successfully" -ForegroundColor Green
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