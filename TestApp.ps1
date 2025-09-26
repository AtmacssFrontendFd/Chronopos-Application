# Test Application Startup
param([switch]$WaitForExit)

Write-Host "Testing ChronoPos Desktop Application Startup..." -ForegroundColor Cyan
Write-Host ""

$exePath = "Deployment\Output\ChronoPos.Desktop.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "✗ Application not found at: $exePath" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Application found: $exePath" -ForegroundColor Green

try {
    Write-Host "Starting application..." -ForegroundColor Yellow
    
    $process = Start-Process -FilePath $exePath -PassThru -WindowStyle Normal
    
    if ($process) {
        Write-Host "✓ Application process started successfully (PID: $($process.Id))" -ForegroundColor Green
        
        # Wait a moment to see if it crashes immediately
        Start-Sleep -Seconds 3
        
        if ($process.HasExited) {
            Write-Host "✗ Application exited immediately (Exit Code: $($process.ExitCode))" -ForegroundColor Red
            if ($process.ExitCode -ne 0) {
                Write-Host "This indicates an error during startup" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✓ Application is running successfully!" -ForegroundColor Green
            Write-Host "The side-by-side configuration issue has been resolved." -ForegroundColor Green
            
            if ($WaitForExit) {
                Write-Host "Waiting for application to exit..." -ForegroundColor Gray
                $process.WaitForExit()
                Write-Host "Application exited with code: $($process.ExitCode)" -ForegroundColor Gray
            } else {
                Write-Host "Terminating test application..." -ForegroundColor Gray
                $process.Kill()
                $process.WaitForExit(5000)
            }
        }
    } else {
        Write-Host "✗ Failed to start application process" -ForegroundColor Red
    }
}
catch {
    Write-Host "✗ Error starting application: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed." -ForegroundColor Cyan