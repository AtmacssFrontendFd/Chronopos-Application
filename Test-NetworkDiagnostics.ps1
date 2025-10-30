# ChronoPos - Network Diagnostic Script
# Run this on both host and client to diagnose network issues

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ChronoPos Network Diagnostics" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get local IP addresses
Write-Host "1. Network Interfaces and IP Addresses:" -ForegroundColor Cyan
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"} | Format-Table InterfaceAlias, IPAddress, PrefixLength -AutoSize

# Check if multicast is enabled
Write-Host ""
Write-Host "2. Multicast Support on Network Adapters:" -ForegroundColor Cyan
Get-NetAdapter | Where-Object {$_.Status -eq "Up"} | ForEach-Object {
    $adapter = $_
    $ipInterface = Get-NetIPInterface -InterfaceIndex $adapter.InterfaceIndex -AddressFamily IPv4
    Write-Host "   - $($adapter.Name): Status=$($adapter.Status), MulticastSupport=$($ipInterface.MulticastEnabled)" -ForegroundColor $(if ($ipInterface.MulticastEnabled) { "Green" } else { "Red" })
}

# Check Windows Firewall status
Write-Host ""
Write-Host "3. Windows Firewall Status:" -ForegroundColor Cyan
$firewallProfiles = Get-NetFirewallProfile
foreach ($profile in $firewallProfiles) {
    $status = if ($profile.Enabled) { "Enabled" } else { "Disabled" }
    $color = if ($profile.Enabled) { "Yellow" } else { "Green" }
    Write-Host "   - $($profile.Name): $status" -ForegroundColor $color
}

# Check for ChronoPos firewall rules
Write-Host ""
Write-Host "4. ChronoPos Firewall Rules:" -ForegroundColor Cyan
$rules = Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*ChronoPos*"}
if ($rules) {
    foreach ($rule in $rules) {
        $ruleDetails = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $rule -ErrorAction SilentlyContinue
        Write-Host "   ✅ $($rule.DisplayName) - Direction: $($rule.Direction), Action: $($rule.Action), Enabled: $($rule.Enabled)" -ForegroundColor Green
        if ($ruleDetails) {
            Write-Host "      Port: $($ruleDetails.LocalPort), Protocol: $($ruleDetails.Protocol)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "   ❌ No ChronoPos firewall rules found!" -ForegroundColor Red
    Write-Host "   ⚠️  Run Add-FirewallRules.ps1 as Administrator" -ForegroundColor Yellow
}

# Test UDP port 42099
Write-Host ""
Write-Host "5. UDP Port 42099 Status:" -ForegroundColor Cyan
$udpConnections = Get-NetUDPEndpoint -LocalPort 42099 -ErrorAction SilentlyContinue
if ($udpConnections) {
    Write-Host "   ✅ Port 42099 is in use:" -ForegroundColor Green
    $udpConnections | Format-Table LocalAddress, LocalPort, OwningProcess -AutoSize
    
    # Get process name
    foreach ($conn in $udpConnections) {
        $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "   Process: $($process.ProcessName) (PID: $($conn.OwningProcess))" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "   ⚠️  Port 42099 is not in use (ChronoPos may not be running)" -ForegroundColor Yellow
}

# Check connectivity to multicast address
Write-Host ""
Write-Host "6. Multicast Configuration:" -ForegroundColor Cyan
Write-Host "   Target: 239.255.42.99:42099" -ForegroundColor Gray
Write-Host "   Multicast range: 239.255.42.0/24" -ForegroundColor Gray

# Check if router supports multicast
Write-Host ""
Write-Host "7. Default Gateway (Router):" -ForegroundColor Cyan
$gateway = Get-NetRoute -DestinationPrefix "0.0.0.0/0" | Select-Object -First 1
if ($gateway) {
    Write-Host "   Gateway: $($gateway.NextHop)" -ForegroundColor Green
    Write-Host "   Interface: $($gateway.InterfaceAlias)" -ForegroundColor Gray
    
    # Test ping to gateway
    $pingResult = Test-Connection -ComputerName $gateway.NextHop -Count 1 -Quiet
    if ($pingResult) {
        Write-Host "   ✅ Can reach gateway" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Cannot reach gateway" -ForegroundColor Red
    }
}

# Show routing table
Write-Host ""
Write-Host "8. Multicast Routing:" -ForegroundColor Cyan
$multicastRoutes = Get-NetRoute -AddressFamily IPv4 | Where-Object {$_.DestinationPrefix -like "224.*" -or $_.DestinationPrefix -like "239.*"}
if ($multicastRoutes) {
    $multicastRoutes | Format-Table DestinationPrefix, NextHop, InterfaceAlias, RouteMetric -AutoSize
} else {
    Write-Host "   ⚠️  No multicast routes found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Diagnostic Summary:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Provide recommendations
Write-Host "Recommendations:" -ForegroundColor Yellow
Write-Host ""

if (-not $rules) {
    Write-Host "❌ CRITICAL: No firewall rules found for ChronoPos" -ForegroundColor Red
    Write-Host "   → Run Add-FirewallRules.ps1 as Administrator" -ForegroundColor Yellow
    Write-Host ""
}

if (-not $udpConnections) {
    Write-Host "⚠️  WARNING: Port 42099 is not in use" -ForegroundColor Yellow
    Write-Host "   → Make sure ChronoPos is running" -ForegroundColor Yellow
    Write-Host ""
}

$firewallEnabled = ($firewallProfiles | Where-Object {$_.Enabled}).Count -gt 0
if ($firewallEnabled -and -not $rules) {
    Write-Host "⚠️  WARNING: Windows Firewall is enabled but no ChronoPos rules exist" -ForegroundColor Yellow
    Write-Host "   → This will block host discovery" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "If host discovery still fails after adding firewall rules:" -ForegroundColor Cyan
Write-Host "1. Check if router has 'AP Isolation' or 'Client Isolation' enabled" -ForegroundColor White
Write-Host "2. Try connecting both devices to router via Ethernet cable" -ForegroundColor White
Write-Host "3. Check router's multicast/IGMP settings" -ForegroundColor White
Write-Host "4. Temporarily disable Windows Firewall to test" -ForegroundColor White
Write-Host ""

pause
