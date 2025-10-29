# ChronoPOS Network Sharing Setup Guide

## Overview

This guide explains how to set up ChronoPOS in network sharing mode, allowing multiple POS terminals to share the same database over a local network.

---

## Architecture

```
┌─────────────────────────────────────────┐
│         HOST DEVICE (e.g. 192.168.1.100)│
│                                         │
│  C:\Users\[User]\AppData\Local\        │
│     ChronoPos\chronopos.db              │
│  (Shared as "ChronoPosDB")              │
│                                         │
│  Requirements:                          │
│  - Valid license with MaxPosDevices > 1 │
│  - Windows File Sharing enabled         │
│  - Folder shared with Read/Write access │
└─────────────────────────────────────────┘
             │
             │ Local Network (WiFi/LAN)
             │
    ┌────────┴────────┬────────────────┐
    │                 │                │
┌───▼────┐      ┌────▼────┐     ┌────▼────┐
│ Client │      │ Client  │     │ Client  │
│ POS 1  │      │ POS 2   │     │ POS 3   │
└────────┘      └─────────┘     └─────────┘
```

---

## Prerequisites

### Host Device Requirements:
- ✅ Windows 10/11 Professional, Enterprise, or Server
- ✅ ChronoPOS installed and activated
- ✅ Valid license (any MaxPosDevices value)
- ✅ Static IP address (recommended)
- ✅ Windows File Sharing enabled

### Client Device Requirements:
- ✅ Windows 10/11 (any edition)
- ✅ ChronoPOS installed (not yet activated)
- ✅ Same local network as host
- ✅ Network access to host device

### Network Requirements:
- ✅ All devices on same subnet (e.g., 192.168.1.x)
- ✅ Firewall allows file sharing (SMB ports 445, 139)
- ✅ No internet required (local network only)

---

## Step-by-Step Setup

### Part 1: Configure Host Device

#### Step 1: Activate ChronoPOS on Host

1. Launch ChronoPOS
2. Complete activation using scratch card
3. License will automatically enable host mode once activated

#### Step 2: Find Database Location

The database is automatically created at:
```
C:\Users\[YourUsername]\AppData\Local\ChronoPos\
```

To find it easily:
1. Press `Win + R`
2. Type: `%LOCALAPPDATA%\ChronoPos`
3. Press Enter
4. You should see `chronopos.db` file

#### Step 3: Share the Folder

**Method A: Using Windows Explorer (Recommended)**

1. Right-click on the `ChronoPos` folder
2. Select **Properties**
3. Go to **Sharing** tab
4. Click **Advanced Sharing**
5. Check ☑ **Share this folder**
6. Set Share name: `ChronoPosDB` (exactly as shown)
7. Click **Permissions**
8. Add users/groups who need access:
   - Option A: `Everyone` with **Full Control** (simple, less secure)
   - Option B: Specific user accounts with **Change** permissions (more secure)
9. Click **OK** on all dialogs

**Method B: Using PowerShell (Advanced)**

Open PowerShell as Administrator:

```powershell
# Get the database folder path
$dbPath = "$env:LOCALAPPDATA\ChronoPos"

# Share the folder
New-SmbShare -Name "ChronoPosDB" -Path $dbPath -FullAccess "Everyone"

# Set NTFS permissions
$acl = Get-Acl $dbPath
$permission = "Everyone","FullControl","Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $dbPath $acl

Write-Host "Folder shared successfully as \\$env:COMPUTERNAME\ChronoPosDB"
```

#### Step 4: Verify Sharing

1. Press `Win + R`
2. Type: `\\localhost\ChronoPosDB`
3. Press Enter
4. You should see the `chronopos.db` file

#### Step 5: Note Your IP Address

1. Open Command Prompt
2. Type: `ipconfig`
3. Look for **IPv4 Address** under your active network adapter
4. Example: `192.168.1.100`
5. **Write this down** - clients will need it

#### Step 6: Configure Firewall (if needed)

If clients can't connect, allow file sharing through firewall:

```powershell
# Run as Administrator
New-NetFirewallRule -DisplayName "ChronoPOS File Sharing" -Direction Inbound -Profile Any -Action Allow -Protocol TCP -LocalPort 445,139
```

---

### Part 2: Configure Client Devices

#### Step 1: Install ChronoPOS

1. Install ChronoPOS on client device
2. **Do NOT activate yet** - we'll connect to host instead

#### Step 2: Connect to Host via Setup Wizard

1. Launch ChronoPOS
2. Setup wizard will appear
3. Choose **"Connect to Host"** option
4. Click **"🔍 Search for Hosts"**
5. Wait 10-15 seconds for discovery
6. Select your host from the list
7. Click **"Connect"**

#### Step 3: Grant Network Access

If prompted "Network credentials required":
1. Enter Windows username and password from **HOST** computer
2. Check ☑ "Remember my credentials"
3. Click **OK**

#### Step 4: Verify Connection

After connection:
- ✅ You should see a success message
- ✅ ChronoPOS will restart
- ✅ Client now uses host's database

---

## Verification & Testing

### Test 1: Database Access

On **client device**:
1. Open File Explorer
2. Navigate to: `\\[HOST_IP]\ChronoPosDB`
3. Verify you can see `chronopos.db`

### Test 2: Real-Time Sync

1. On **client**: Add a new product
2. On **host**: Verify product appears immediately
3. On **another client**: Verify product is visible

### Test 3: Concurrent Operations

1. Process a sale on client 1
2. Simultaneously process a sale on client 2
3. Both should complete without errors

---

## Troubleshooting

### Problem: "Host not found during discovery"

**Solutions:**
- ✅ Ensure both devices are on same network
- ✅ Disable VPN on both devices
- ✅ Check firewall isn't blocking UDP port 42099
- ✅ Restart host's ChronoPOS to restart broadcasting

**Manual connection:**
1. Note host's IP address (e.g., 192.168.1.100)
2. On client, open connection config:
   - Path: `%LOCALAPPDATA%\ChronoPos\connection.json`
3. Manually create config (see example below)

### Problem: "Cannot access database path"

**Solutions:**
- ✅ Verify folder is shared as `ChronoPosDB`
- ✅ Test access: `\\[HOST_IP]\ChronoPosDB` from client
- ✅ Check permissions (need Read + Write)
- ✅ Try with `Everyone` full control first (troubleshooting)

**Verify sharing with PowerShell:**
```powershell
Get-SmbShare -Name "ChronoPosDB"
```

### Problem: "Database is locked" errors

**Solutions:**
- ✅ Ensure WAL mode is enabled (automatic)
- ✅ Check network stability
- ✅ Reduce number of clients (max 5-6 recommended)
- ✅ Use wired Ethernet instead of WiFi

### Problem: Slow performance

**Solutions:**
- ✅ Use Ethernet cables instead of WiFi
- ✅ Upgrade network equipment (gigabit switch/router)
- ✅ Close unnecessary applications
- ✅ Limit to 5-6 clients maximum

### Problem: Connection drops randomly

**Solutions:**
- ✅ Set static IP on host device
- ✅ Disable power saving on network adapters
- ✅ Check network cable connections
- ✅ Update network drivers

---

## Manual Configuration

### Manual Connection Config

If automatic connection fails, create manually:

**Location:** `C:\Users\[User]\AppData\Local\ChronoPos\connection.json`

```json
{
  "IsClient": true,
  "IsHost": false,
  "HostIp": "192.168.1.100",
  "DatabasePath": "\\\\192.168.1.100\\ChronoPosDB\\chronopos.db",
  "Token": {
    "Token": "generated-token-here",
    "HostIp": "192.168.1.100",
    "HostName": "HOST-PC-NAME",
    "DatabaseUncPath": "\\\\192.168.1.100\\ChronoPosDB\\chronopos.db",
    "IssuedAt": "2025-10-29T10:00:00Z",
    "ExpiresAt": "2026-10-29T10:00:00Z",
    "ClientFingerprint": "your-machine-fingerprint",
    "PlanId": 2,
    "MaxPosDevices": 5
  },
  "ConfiguredAt": "2025-10-29T10:00:00Z"
}
```

Replace:
- `192.168.1.100` with your host's IP
- `HOST-PC-NAME` with host's computer name

---

## Performance Optimization

### Host Device

1. **Use SSD** for database storage
2. **Disable indexing** on database folder:
   - Right-click folder → Properties
   - Uncheck "Allow files in this folder to have contents indexed"
3. **Increase SMB cache:**
   ```powershell
   Set-SmbServerConfiguration -MaxSessionPerConnection 16384
   ```

### Network

1. **Use Gigabit Ethernet** (avoid WiFi for host)
2. **Quality router/switch** (avoid cheap consumer models)
3. **Short cables** (< 10 meters)
4. **Separate network** for POS devices (optional but recommended)

### Database

WAL mode is automatically enabled, but verify:

1. Connect to database
2. Run: `PRAGMA journal_mode;`
3. Should return: `wal`

---

## Security Best Practices

### ⚠️ Production Environments

1. **Don't use "Everyone" in production**
   - Create specific user accounts
   - Grant minimal permissions needed

2. **Use Windows domain** (if available)
   - Better user management
   - Centralized access control

3. **Backup regularly**
   - Host device should backup database daily
   - Store backups off-site

4. **Monitor access**
   - Check event logs periodically
   - Watch for unusual activity

---

### Limitations

### SQLite Over Network

⚠️ **Important Limitations:**

1. **Maximum 5-6 clients recommended** (not enforced by license)
   - More clients = slower performance
   - Higher risk of locking issues
   - No hard limit on client connections

2. **Network dependency**
   - If host goes down, all clients stop
   - If network fails, all clients stop
   - Not truly "offline" (needs LAN)

3. **No geographic distribution**
   - All devices must be on same local network
   - Won't work across internet/VPN reliably

### When to Upgrade

Consider upgrading to API-based architecture if:
- More than 6 terminals needed
- Frequent database lock errors
- Slow query performance
- Need truly offline capability per device

---

## Support

### Logs Location

**Host:** `%LOCALAPPDATA%\ChronoPos\app.log`
**Client:** `%LOCALAPPDATA%\ChronoPos\app.log`

### Check Connection Status

Look for these log entries:
- "HOST MODE - Using local database"
- "CLIENT MODE - Using network database"
- "Host broadcasting started successfully"
- "Connected successfully"

### Common Log Messages

✅ **Success:**
```
Broadcasting as: MAIN-PC (192.168.1.100)
Database configuration completed
WAL mode enabled
```

❌ **Issues:**
```
Cannot access database at \\192.168.1.100\ChronoPosDB\chronopos.db
Database connection test failed
Network path validation: Not accessible
```

---

## FAQ

**Q: Can clients work if host is turned off?**
A: No. All clients require the host to be running and accessible.

**Q: Does this work over internet?**
A: No. This is designed for local network only (WiFi/LAN).

**Q: How many clients can connect?**
A: No hard limit enforced. However, we recommend max 5-6 clients for optimal performance with SQLite over network.

**Q: Can I use WiFi for all devices?**
A: Yes, but Ethernet recommended for host device for better reliability.

**Q: What if I change host's IP?**
A: Clients will need to reconnect or update connection.json manually.

**Q: Is my data secure?**
A: Data is transmitted over local network. Use proper Windows permissions and network security.

---

## Quick Reference

### Host Setup Checklist
- [ ] ChronoPOS activated with valid license
- [ ] Folder shared as "ChronoPosDB"
- [ ] Read/Write permissions granted
- [ ] Firewall allows file sharing
- [ ] IP address noted
- [ ] Broadcasting started automatically (check logs)

### Client Setup Checklist
- [ ] ChronoPOS installed
- [ ] On same network as host
- [ ] Can access \\\\[HOST_IP]\\ChronoPosDB
- [ ] Connected via setup wizard
- [ ] Success message received
- [ ] Application restarted

---

**Document Version:** 1.0  
**Last Updated:** October 29, 2025  
**ChronoPOS Version:** Compatible with all versions supporting network sharing
