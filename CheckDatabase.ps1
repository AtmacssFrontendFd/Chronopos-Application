# Simple PowerShell script to check SQLite database
$dbPath = "C:\Users\saswa\AppData\Local\ChronoPos\chronopos.db"

# Download sqlite3.exe if not available
$sqliteUrl = "https://www.sqlite.org/2023/sqlite-tools-win32-x86-3420000.zip"
$tempDir = "$env:TEMP\sqlite"
$sqliteExe = "$tempDir\sqlite3.exe"

if (!(Test-Path $sqliteExe)) {
    Write-Host "Downloading SQLite tools..."
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    $zipFile = "$tempDir\sqlite-tools.zip"
    Invoke-WebRequest -Uri $sqliteUrl -OutFile $zipFile
    Expand-Archive -Path $zipFile -DestinationPath $tempDir -Force
    Move-Item "$tempDir\sqlite-tools-win32-x86-3420000\sqlite3.exe" $sqliteExe -Force
}

Write-Host "Checking database tables..."
& $sqliteExe $dbPath ".tables"

Write-Host "`nChecking Customer table schema..."
& $sqliteExe $dbPath ".schema Customer"

Write-Host "`nChecking Customer data..."
& $sqliteExe $dbPath "SELECT COUNT(*) as CustomerCount FROM Customer;"

Write-Host "`nChecking BusinessType table..."
& $sqliteExe $dbPath "SELECT * FROM BusinessType LIMIT 5;" 2>$null

Write-Host "`nFirst few customers..."
& $sqliteExe $dbPath "SELECT Id, CustomerFullName, BusinessFullName, IsBusiness, MobileNo FROM Customer LIMIT 5;" 2>$null