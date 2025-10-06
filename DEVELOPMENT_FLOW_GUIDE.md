# ChronoPOS Application Flow & Development Guide

## Application Startup Flow

### First Run (Fresh Installation)
1. **Onboarding Wizard** → License activation with QR/manual entry
2. **Create Admin User** → Set up administrator account  
3. **Login** → Authenticate with admin credentials
4. **Dashboard** → Main POS application

### Subsequent Runs
1. **Login** → Authenticate with existing user credentials
2. **Dashboard** → Main POS application

## Data Persistence

All application data is stored in two locations:

### 1. User AppData Folder
**Location:** `%LOCALAPPDATA%\ChronoPos`

**Contents:**
- `License/` - License activation files
- `Licensing/` - Licensing service data
- `Admin/` - Admin panel data (if installed)
- `username.dat` - Saved username for "Remember Me" feature
- `app.log` - Application logs
- `chronopos-admin.db` - Admin panel database (if installed)

### 2. Application Folder
**Location:** `E:\WORK\ChronoPosRevised\src\ChronoPos.Infrastructure\`

**Contents:**
- `chronopos.db` - Main SQLite database (Users, Products, Sales, etc.)

## Development Workflow

### Clear All Data (Fresh Start)

Run the cleanup script to clear all persisted data:

```powershell
.\clear_dev_data.ps1
```

This will:
- Delete the ChronoPos AppData folder
- Delete the main database file
- Give you a completely fresh start

### Manual Cleanup

**Clear License Data:**
```powershell
Remove-Item "$env:LOCALAPPDATA\ChronoPos\License" -Recurse -Force
Remove-Item "$env:LOCALAPPDATA\ChronoPos\Licensing" -Recurse -Force
```

**Clear Database:**
```powershell
Remove-Item "E:\WORK\ChronoPosRevised\src\ChronoPos.Infrastructure\chronopos.db" -Force
```

**Clear Saved Username:**
```powershell
Remove-Item "$env:LOCALAPPDATA\ChronoPos\username.dat" -Force
```

## User Authentication

### Admin User Creation

After successful license activation, you'll be prompted to create an admin user:

**Required Fields:**
- Full Name
- Email Address  
- Username (for login)
- Password (min 6 characters)
- Confirm Password

**Security:**
- Passwords are hashed using SHA-256
- Username is saved locally for "Remember Me" feature
- User data is stored in the `Users` table

### Login Process

**Credentials:**
- Username/Email: The email you used when creating the admin user
- Password: The password you set

**Features:**
- "Remember Me" saves your username locally
- Enter key support for quick login
- Clear error messages for invalid credentials

## Window Sizes

All windows are designed to be consistent:

- **Width:** 1200px
- **Height:** 800px  
- **Style:** Borderless with rounded corners
- **Position:** Center screen

## Testing the Complete Flow

### 1. Test Fresh Installation

```powershell
# Clear all data
.\clear_dev_data.ps1

# Run application
dotnet run --project src\ChronoPos.Desktop\ChronoPos.Desktop.csproj
```

**Expected Flow:**
1. Onboarding window appears
2. Enter/scan license activation code
3. Confirm salesperson details
4. Enter business information  
5. Generate sales key
6. Activate license
7. Success screen → Click "Get Started"
8. Create Admin window appears
9. Fill in admin details and create
10. Login window appears
11. Enter admin credentials
12. Dashboard opens

### 2. Test Subsequent Runs

```powershell
# Run application (with existing data)
dotnet run --project src\ChronoPos.Desktop\ChronoPos.Desktop.csproj
```

**Expected Flow:**
1. Login window appears
2. Enter credentials
3. Dashboard opens

### 3. Test Without License

```powershell
# Clear only license data
Remove-Item "$env:LOCALAPPDATA\ChronoPos\License" -Recurse -Force

# Run application
dotnet run --project src\ChronoPos.Desktop\ChronoPos.Desktop.csproj
```

**Expected Flow:**
1. Onboarding window appears (license re-activation required)

## Troubleshooting

### "Login window appears but I never created a user"

**Cause:** Previous session data is persisting

**Solution:**
```powershell
.\clear_dev_data.ps1
```

### "Dashboard doesn't open after clicking Get Started"

**Cause:** OnboardingCompleted event not properly wired

**Check:** 
- OnboardingViewModel.CompleteOnboardingCommand invokes OnboardingCompleted event
- OnboardingWindow.xaml.cs subscribes to the event  
- App.xaml.cs checks DialogResult and shows CreateAdmin or Login

### "I forgot my admin password"

**Solution:** Clear the database and recreate the admin user
```powershell
Remove-Item "E:\WORK\ChronoPosRevised\src\ChronoPos.Infrastructure\chronopos.db" -Force
```

## Database Schema

### Users Table

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY,
    Deleted BOOLEAN DEFAULT 0,
    FullName TEXT NOT NULL,
    Email TEXT NOT NULL,
    Password TEXT NOT NULL,  -- SHA-256 hashed
    Role TEXT,
    PhoneNo TEXT,
    RolePermissionId INTEGER,
    ShopId INTEGER,
    CreatedAt DATETIME
);
```

## Security Notes

- Passwords are hashed with SHA-256 (consider upgrading to bcrypt/Argon2 for production)
- License files are encrypted
- Machine fingerprinting binds licenses to specific hardware
- No plaintext passwords are stored anywhere

## Next Steps

- [ ] Implement password reset functionality
- [ ] Add multi-user support
- [ ] Implement role-based permissions
- [ ] Add session timeout/auto-logout
- [ ] Implement audit logging for user actions
