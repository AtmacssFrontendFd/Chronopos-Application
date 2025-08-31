# 🚀 ChronoPos Desktop POS - Quick Start Guide

## Prerequisites Check ✅

Before running the project, ensure you have the following installed:

### 1. **Required Software**

- **Windows 10 version 1903** or later (Windows 11 recommended)
- **.NET 9 SDK** - Download from: https://dotnet.microsoft.com/download/dotnet/9.0

### 2. **Verify Installation**

Open PowerShell and run:

```powershell
# Check .NET version
dotnet --version
# Should show: 9.0.xxx
```

**Note**: No additional database installation required! ChronoPos uses SQLite which works out of the box.

---

## 🏃‍♂️ Quick Start (2 Steps)

### Step 1: Navigate to Project Directory

```powershell
cd "c:\Users\saswa\OneDrive\Desktop\chronopos"
```

### Step 2: Run the Application

**Option A: Simple Launcher (Recommended)**

```powershell
.\simple_run.bat
```

**Option B: Direct Run**

```powershell
dotnet run --project src\ChronoPos.Desktop
```

**That's it!** The application will automatically:

- Build the solution
- Create the SQLite database in your local AppData folder
- Initialize with sample data (categories, products, customers)
- Launch the POS interface

### Step 5: Run the Desktop Application

```powershell
dotnet run --project src\ChronoPos.Desktop
```

**🎉 Your ChronoPos Desktop POS application should now be running!**

---

## 🔧 Troubleshooting

### Problem: ".NET 9 not found"

**Solution:**

1. Download and install .NET 9 SDK from Microsoft
2. Restart PowerShell
3. Verify with: `dotnet --version`

### Problem: "LocalDB not available"

**Solution:**

```powershell
# Install SQL Server Express LocalDB
# Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
# Or install via Visual Studio Installer
```

### Problem: "Migration fails"

**Solution:**

```powershell
# Delete existing database and recreate
dotnet ef database drop --startup-project src\ChronoPos.Desktop --force
dotnet ef database update --startup-project src\ChronoPos.Desktop
```

### Problem: "Build errors"

**Solution:**

```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 📋 Testing the Application

### Run Unit Tests

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test tests\ChronoPos.Domain.Tests
```

### Test Database Connection

```powershell
# Verify database was created successfully
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases WHERE name = 'ChronoPosDb'"
```

---

## 🎯 Using the Application

Once running, you'll see the ChronoPos main window with:

1. **📊 Dashboard** - Business overview and statistics
2. **🛒 Point of Sale** - Main transaction interface
3. **📦 Products** - Inventory management
4. **👥 Customers** - Customer database
5. **📈 Sales History** - Transaction reports
6. **⚙️ Settings** - Application configuration

---

## 🚀 Development Mode

### For Development with Hot Reload

```powershell
# Run in development mode with file watching
dotnet watch run --project src\ChronoPos.Desktop
```

### Database Management

```powershell
# Add new migration after model changes
dotnet ef migrations add YourMigrationName --startup-project src\ChronoPos.Desktop

# Update database with new migration
dotnet ef database update --startup-project src\ChronoPos.Desktop

# Reset database (WARNING: Deletes all data)
dotnet ef database drop --startup-project src\ChronoPos.Desktop --force
dotnet ef database update --startup-project src\ChronoPos.Desktop
```

---

## 📦 Building for Distribution

### Create Release Build

```powershell
# Build optimized release version
dotnet publish src\ChronoPos.Desktop -c Release -r win-x64 --self-contained

# Output will be in: src\ChronoPos.Desktop\bin\Release\net9.0-windows\win-x64\publish\
```

### Create Installer (Optional)

After building, you can use tools like:

- **Inno Setup** for Windows installer
- **WiX Toolset** for MSI packages
- **ClickOnce** for automatic updates

---

## 📞 Support

If you encounter any issues:

1. **Check Prerequisites** - Ensure .NET 9 and LocalDB are installed
2. **Clean Build** - Run `dotnet clean` then `dotnet build`
3. **Reset Database** - Drop and recreate the database
4. **Check Logs** - Look for error messages in the PowerShell output

---

## 🎉 Success!

If everything is working correctly, you should see:

- ChronoPos desktop application window
- Professional POS interface with navigation sidebar
- Dashboard showing "Welcome to ChronoPos Point of Sale System"
- All menu items (Dashboard, POS, Products, etc.) clickable and functional

**Your Windows Desktop Point of Sale system is now ready for business! 💼**
