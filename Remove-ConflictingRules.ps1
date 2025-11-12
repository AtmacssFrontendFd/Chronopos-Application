# Remove conflicting ChronoPos firewall rules
# Run as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Removing Conflicting Firewall Rules" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check admin
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    pause
    exit 1
}

# Get all ChronoPos rules
$allRules = Get-NetFirewallRule | Where-Object {
    $_.DisplayName -like "*chronopos*" -and 
    $_.Action -eq "Block"
}

if ($allRules.Count -eq 0) {
    Write-Host "✅ No conflicting BLOCK rules found" -ForegroundColor Green
} else {
    Write-Host "Found $($allRules.Count) BLOCK rules to remove:" -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($rule in $allRules) {
        Write-Host "   Removing: $($rule.DisplayName) (Action: $($rule.Action))" -ForegroundColor Yellow
        Remove-NetFirewallRule -Name $rule.Name
        Write-Host "   ✅ Removed" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Current ChronoPos ALLOW rules:" -ForegroundColor Cyan
$allowRules = Get-NetFirewallRule | Where-Object {
    $_.DisplayName -like "*ChronoPos*" -and 
    $_.Action -eq "Allow"
}

foreach ($rule in $allowRules) {
    $portFilter = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $rule -ErrorAction SilentlyContinue
    if ($portFilter) {
        Write-Host "   ✅ $($rule.DisplayName) - Port: $($portFilter.LocalPort), Protocol: $($portFilter.Protocol)" -ForegroundColor Green
    }
}

Write-Host ""
pause
