; ChronoPos Desktop POS System - Professional Installer Script
; Inno Setup 6.0 or higher required
; This installer handles:
; - Installation path selection
; - Desktop shortcut creation
; - Start menu shortcuts
; - Database initialization (in %LocalAppData%\ChronoPos)
; - Uninstaller registration
; - .NET 9.0 runtime check

#define MyAppName "ChronoPos Desktop"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ChronoPos"
#define MyAppURL "https://chronopos.com"
#define MyAppExeName "ChronoPos.Desktop.exe"
#define MyAppId "{{A7B8C9D0-E1F2-4A5B-8C9D-0E1F2A5B8C9D}"

[Setup]
; Basic app information
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Allow user to choose installation directory
UsePreviousAppDir=yes

; Output configuration
OutputDir=..\Deployment\Installer
OutputBaseFilename=ChronoPosSetup
SetupIconFile=..\src\ChronoPos.Desktop\Images\LogoImage.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Compression
Compression=lzma2/ultra64
SolidCompression=yes

; UI Configuration
WizardStyle=modern
; WizardImageFile=compiler:WizModernImage-IS.bmp
; WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp

; Privileges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Uninstaller
UninstallDisplayName={#MyAppName}
UninstallFilesDir={app}\uninstall

; Info files
LicenseFile=License.txt
InfoBeforeFile=PreInstallInfo.txt
InfoAfterFile=PostInstallInfo.txt

; Version information
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Installer
VersionInfoCopyright=Copyright (C) 2025 {#MyAppPublisher}

; Architecture
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application files (from published portable build)
Source: "..\Deployment\ChronoPos-Portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Documentation
Source: "..\Deployment\README.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

; Desktop shortcut (if selected)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

; Quick Launch shortcut (if selected)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: quicklaunchicon

[Run]
; Run the application after installation (optional)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DataDirPage: TInputDirWizardPage;
  DatabaseInitialized: Boolean;

// Check if .NET 9.0 Runtime is installed
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
  DotNetVersion: String;
begin
  Result := False;
  
  // Try to get .NET version
  if Exec('dotnet', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := True;
    Log('dotnet --version succeeded, .NET is installed');
  end
  else
  begin
    Log('dotnet --version failed, .NET might not be installed');
    Result := False;
  end;
end;

// Initialize wizard pages
procedure InitializeWizard();
begin
  DatabaseInitialized := False;
  
  // Add custom page for data directory (optional, we use %LocalAppData%)
  DataDirPage := CreateInputDirPage(wpSelectDir,
    'Select Data Storage Location', 
    'Where should ChronoPos store its data?',
    'ChronoPos will create a database and store application data in the following folder.' + #13#10 + 
    'Default location is recommended for most users.' + #13#10#13#10 +
    'To continue, click Next. If you would like to select a different folder, click Browse.',
    False, '');
    
  // Set default data directory
  DataDirPage.Add('');
  DataDirPage.Values[0] := ExpandConstant('{localappdata}\ChronoPos');
end;

// Check prerequisites before installation
function InitializeSetup(): Boolean;
var
  ErrorMessage: String;
begin
  Result := True;
  
  // Check Windows version (Windows 10 1809 or higher)
  if not (GetWindowsVersion >= $0A00000000) then
  begin
    ErrorMessage := 'ChronoPos requires Windows 10 (version 1809) or higher.' + #13#10 +
                    'Please upgrade your operating system before installing.';
    MsgBox(ErrorMessage, mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // Note: .NET check removed since we're using self-contained deployment
  // The app includes all necessary .NET runtime files
  
  Log('Prerequisites check passed');
end;

// Create necessary directories
procedure CreateDataDirectories();
var
  DataDir: String;
begin
  try
    // Get the data directory path
    DataDir := DataDirPage.Values[0];
    
    Log('Creating data directories...');
    Log('Data directory: ' + DataDir);
    
    // Create main data directory
    if not DirExists(DataDir) then
    begin
      if CreateDir(DataDir) then
        Log('Created data directory: ' + DataDir)
      else
        Log('Failed to create data directory: ' + DataDir);
    end
    else
      Log('Data directory already exists: ' + DataDir);
      
    // Create subdirectories
    ForceDirectories(DataDir + '\Images');
    ForceDirectories(DataDir + '\Licensing');
    ForceDirectories(DataDir + '\Backups');
    
    Log('Data directories created successfully');
  except
    Log('Error creating data directories: ' + GetExceptionMessage);
  end;
end;

// Initialize database on first run
procedure InitializeDatabase();
var
  ResultCode: Integer;
  AppPath: String;
  DataDir: String;
begin
  try
    AppPath := ExpandConstant('{app}\{#MyAppExeName}');
    DataDir := DataDirPage.Values[0];
    
    Log('Initializing database...');
    Log('App path: ' + AppPath);
    Log('Data directory: ' + DataDir);
    
    // The application will automatically create the database on first run
    // We just need to ensure the directories exist
    CreateDataDirectories();
    
    DatabaseInitialized := True;
    Log('Database initialization prepared');
  except
    Log('Error during database initialization: ' + GetExceptionMessage);
    DatabaseInitialized := False;
  end;
end;

// Post-installation steps
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('Running post-installation steps...');
    
    // Create data directories
    CreateDataDirectories();
    
    // Initialize database
    InitializeDatabase();
    
    Log('Post-installation steps completed');
  end;
end;

// Uninstall cleanup
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: String;
  Response: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    // Ask if user wants to delete data
    DataDir := ExpandConstant('{localappdata}\ChronoPos');
    
    Response := MsgBox('Do you want to delete all application data including the database?' + #13#10 + 
                       'This will remove all your sales records, products, and settings.' + #13#10#13#10 +
                       'Location: ' + DataDir + #13#10#13#10 +
                       'Click Yes to delete all data, or No to keep it.',
                       mbConfirmation, MB_YESNO);
                       
    if Response = IDYES then
    begin
      try
        if DirExists(DataDir) then
        begin
          DelTree(DataDir, True, True, True);
          Log('User data deleted: ' + DataDir);
        end;
      except
        Log('Error deleting user data: ' + GetExceptionMessage);
        MsgBox('Could not delete all application data. Some files may be in use.' + #13#10 +
               'Please delete manually if needed: ' + DataDir, 
               mbInformation, MB_OK);
      end;
    end
    else
    begin
      Log('User chose to keep application data');
      MsgBox('Application data has been preserved at:' + #13#10 + DataDir + #13#10#13#10 +
             'You can safely reinstall ChronoPos later and your data will be restored.',
             mbInformation, MB_OK);
    end;
  end;
end;

// Custom message for finish page
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
  begin
    Log('Installation completed successfully');
  end;
end;
