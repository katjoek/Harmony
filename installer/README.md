# Harmony Installer

This directory contains the NSIS installer script for Harmony Community Manager.

## License

Harmony Community Manager is licensed under the Apache License, Version 2.0. The license text is included in `LICENSE.txt` and is displayed during installation. See the root [LICENSE](../LICENSE) file for the full license text.

## Prerequisites

1. **NSIS (Nullsoft Scriptable Install System)** must be installed
   - Download from: https://nsis.sourceforge.io/Download
   - Ensure `makensis.exe` is in your system PATH

2. **.NET 9.0 SDK** must be installed for publishing the application

## Building the Installer

### Option 1: Using the PowerShell Script (Recommended)

Run the build script from the `installer` directory:

```powershell
cd installer
.\build-installer.ps1
```

This script will:
1. Publish the Harmony.Web application in Release configuration
2. Extract the version number from `Directory.Build.props`
3. Build the NSIS installer using the published files
4. Output `Harmony-Setup-{version}.exe` (e.g., `Harmony-Setup-1.0.4.exe`) in the installer directory

### Option 2: Manual Build

1. **Publish the application:**
   ```powershell
   dotnet publish ..\src\Harmony.Web\Harmony.Web.csproj -c Release -r win-x64 --self-contained true
   ```

2. **Build the installer:**
   ```powershell
   # Extract version from Directory.Build.props (optional, defaults to 1.0.0)
   $version = ([xml](Get-Content ..\Directory.Build.props)).Project.PropertyGroup.Version
   makensis /DVERSION="$version" Harmony.nsi
   ```
   
   Or without version (will use default 1.0.0):
   ```powershell
   makensis Harmony.nsi
   ```

## Installer Output

The installer filename includes the application version number from `Directory.Build.props`:
- Example: `Harmony-Setup-1.0.4.exe` (for version 1.0.4)

The version is automatically extracted from `Directory.Build.props` during the build process.

## Installer Features

The installer includes:

- **Application Files**: Installs all published files to `Program Files\Harmony`
- **Desktop Shortcut**: Optional desktop shortcut to launch the application
- **Start Menu Shortcuts**: Optional Start Menu entry with application and uninstall shortcuts
- **Database Directory**: Creates `%APPDATA%\Harmony` directory for database storage
- **Uninstaller**: Complete uninstallation support via Add/Remove Programs

## Installation Path

Default installation path: `C:\Program Files\Harmony`

## Database Location

The application database is stored in: `%APPDATA%\Harmony\harmony.db`

Note: The database directory is preserved during uninstallation to protect user data.

## Customization

To customize the installer:

1. Edit `Harmony.nsi` to modify:
   - Installation path
   - Shortcut locations
   - Version information
   - Additional installation steps

2. Update version information in:
   - `Directory.Build.props` (Version, FileVersion) - this is automatically used by the installer
   - The installer version is automatically synced from `Directory.Build.props` during build

## Troubleshooting

### NSIS not found
- Ensure NSIS is installed and `makensis.exe` is in your PATH
- You can verify by running: `makensis /VERSION`

### Publish output not found
- Ensure you've published the application first
- Check that the publish path matches the expected location:
  `src\Harmony.Web\bin\Release\net9.0\win-x64\publish`

### Installer fails to build
- Check NSIS syntax errors in the output
- Verify all file paths in `Harmony.nsi` are correct relative to the installer directory

