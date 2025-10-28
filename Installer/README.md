# ChronoPos Desktop Installer

This folder contains the Inno Setup installer configuration for ChronoPos Desktop POS System.

## Building the Installer

### Prerequisites

1. **Inno Setup 6** - Download and install from:
   - https://jrsoftware.org/isdl.php
   - Install to default location: `C:\Program Files (x86)\Inno Setup 6\`

2. **Portable Build** - Must build portable version first:
   ```powershell
   .\BuildPortable.ps1
   ```

### Build Process

**Option 1: Automated (Recommended)**
```powershell
.\BuildCompletePackage.ps1
```
This will:
- Build portable version
- Compile installer with Inno Setup
- Generate deployment documentation
- Create both installer EXE and portable ZIP

**Option 2: Manual**
```powershell
# 1. Build portable first
.\BuildPortable.ps1

# 2. Compile installer
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" Installer\ChronoPosSetup.iss
```

## Installer Features

### What Gets Installed

1. **Application Files**
   - Location: `C:\Program Files\ChronoPos Desktop\` (user can customize)
   - All runtime files (~170 MB)
   - Self-contained (includes .NET 9.0 runtime)

2. **Database Initialization**
   - Automatic database creation on first run
   - Location: `%LocalAppData%\ChronoPos\chronopos.db`
   - Automatic directory structure creation:
     - `%LocalAppData%\ChronoPos\Images\`
     - `%LocalAppData%\ChronoPos\Licensing\`
     - `%LocalAppData%\ChronoPos\Backups\`

3. **Shortcuts**
   - Desktop shortcut (optional)
   - Start Menu shortcuts
   - Quick Launch icon (optional)

4. **Uninstaller**
   - Integrated Windows uninstaller
   - Option to preserve or delete data on uninstall

### Installation Process

User experience:
1. Welcome screen with system requirements
2. License agreement
3. Installation location selection
4. Shortcut options
5. Installation progress
6. Completion with post-install instructions

### Post-Installation

On first launch:
1. License activation screen
2. Business information entry
3. Admin account creation
4. Database automatic initialization
5. Ready to use

## Files in This Folder

- `ChronoPosSetup.iss` - Main Inno Setup script
- `License.txt` - EULA shown during installation
- `PreInstallInfo.txt` - Pre-installation information
- `PostInstallInfo.txt` - Post-installation instructions
- `README.md` - This file

## Output Files

After building:

```
Deployment/
├── Installer/
│   └── ChronoPosSetup.exe      (Professional installer, ~73 MB)
├── ChronoPos-Portable.zip      (Portable version, ~72 MB)
├── DEPLOYMENT_GUIDE.txt        (Comprehensive deployment docs)
└── README.txt                  (End-user instructions)
```

## Customization

### Changing Version Number

Edit `ChronoPosSetup.iss`:
```pascal
#define MyAppVersion "1.0.0"  // Change this line
```

### Changing Default Install Location

Edit `ChronoPosSetup.iss`:
```pascal
DefaultDirName={autopf}\{#MyAppName}  // Current default
// Change to custom path if needed
```

### Adding Additional Files

Edit `ChronoPosSetup.iss` [Files] section:
```pascal
[Files]
Source: "path\to\file"; DestDir: "{app}"; Flags: ignoreversion
```

## Troubleshooting

### "Inno Setup not found"
- Install Inno Setup 6 from official website
- Ensure installed to default location
- Restart PowerShell after installation

### "Portable build not found"
- Run `.\BuildPortable.ps1` first
- Check `Deployment\ChronoPos-Portable\` exists
- Ensure build completed successfully

### "Source files not found"
- Verify portable build completed
- Check all files exist in `Deployment\ChronoPos-Portable\`
- Re-run portable build if needed

### Icon Issues
- Verify `src\ChronoPos.Desktop\Images\LogoImage.ico` exists
- Icon must be proper .ico format
- Rebuild if icon was changed

## Distribution

### For Clients (Recommended)
1. Upload `ChronoPosSetup.exe` to file server or cloud
2. Share download link with clients
3. Provide sales key for license activation
4. Include installation guide

### Alternative (Portable)
1. Share `ChronoPos-Portable.zip`
2. No installation required
3. User extracts and runs
4. Still requires license activation

## Testing

Before deploying to clients:

1. **Test on clean Windows 10/11 VM**
   - Run installer as administrator
   - Choose custom installation path
   - Verify all shortcuts created
   - Launch application
   - Check database created at `%LocalAppData%\ChronoPos\`
   - Complete license activation
   - Test basic functionality

2. **Test Uninstaller**
   - Uninstall via Windows Settings
   - Verify data preservation option works
   - Check complete removal if data deleted

3. **Test Upgrade**
   - Install version 1.0.0
   - Install version 1.0.1 over it
   - Verify data preserved
   - Check application updated

## Support

For build or installer issues:
- Email: dev@chronopos.com
- Include error logs
- Provide system information
- Describe steps to reproduce

## License

Copyright (C) 2025 ChronoPos. All rights reserved.
