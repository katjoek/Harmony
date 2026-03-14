# Harmony - Community Management

A modern web application for managing persons and groups within a (church) community, built with Clean Architecture and CQRS principles.

## 🏗️ Architecture

The project follows Clean Architecture principles with the following layers:

- **Domain**: Core logic and business rules
- **ApplicationCore**: Use cases and CQRS implementation
- **Infrastructure**: Data access and external services
- **Web**: Blazor Server UI
- **Tests**: Unit tests for all layers

## 🚀 Technologies

- **.NET 10.0**: Modern framework for C#
- **Blazor Server**: Interactive web application
- **BootstrapBlazor**: UI components
- **Entity Framework Core**: ORM for database access
- **SQLite**: Embedded database
- **LiteBus**: CQRS implementation
- **xUnit**: Unit testing framework
- **Playwright**: E2E browser testing

## 🎯 Features

### Person Management
- ✅ Add, edit, and delete persons
- ✅ First name (required), prefix, surname
- ✅ Date of birth, address, phone, email
- ✅ Manage group memberships
- ✅ Cascading delete on removal

### Group Management
- ✅ Create, edit, and delete groups
- ✅ Unique group name (required)
- ✅ Assign coordinator (optional)
- ✅ Add and remove members
- ✅ Non-cascading delete on removal

### User Interface
- ✅ Dutch interface
- ✅ Professional and user-friendly design
- ✅ Confirmation dialogs for deletion
- ✅ Responsive design for various screen sizes

## 🛠️ Installation and Usage

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code

### Installation
```bash
# Clone the repository
git clone [repository-url]
cd Harmony2

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the application
cd Harmony.Web
dotnet run
```

### Database
The application uses SQLite with automatic database creation. The database is created on the first start.

### Running Tests

#### Unit Tests
```bash
# Run unit tests
dotnet test test/Harmony.Tests

# Run with coverage
dotnet test test/Harmony.Tests --collect:"XPlat Code Coverage"
```

#### E2E Tests (Playwright)
The E2E tests use Microsoft Playwright to test the application in a real browser.

**First-time setup:**
```bash
# Build the E2E test project
dotnet build test/Harmony.E2ETests

# Install Playwright browsers (only needed once)
pwsh test/Harmony.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

**Running tests:**
```bash
# Run E2E tests
dotnet test test/Harmony.E2ETests

# Run with detailed output
dotnet test test/Harmony.E2ETests --logger "console;verbosity=detailed"
```

**Debugging:** For visual debugging, change `Headless = true` to `Headless = false` in `test/Harmony.E2ETests/Infrastructure/PlaywrightFixture.cs`.

#### All Tests
```bash
# Run all tests (unit + E2E)
dotnet test
```

## 📁 Project Structure

```
Harmony2/
├── Harmony.Domain/              # Domain layer
│   ├── Entities/               # Domain entities
│   └── ValueObjects/           # Value objects
├── Harmony.ApplicationCore/     # Application layer
│   ├── Commands/               # CQRS commands
│   ├── Queries/                # CQRS queries
│   ├── DTOs/                   # Data transfer objects
│   └── Interfaces/             # Repository interfaces
├── Harmony.Infrastructure/      # Infrastructure layer
│   ├── Data/                   # EF Core DbContext
│   └── Repositories/           # Repository implementations
├── Harmony.Web/                # Presentation layer
│   ├── Pages/                  # Blazor pages
│   ├── Services/               # Application services (ReportService, ReportFileNameBuilder, …)
│   ├── Shared/                 # Shared components
│   └── wwwroot/                # Static files
├── test/
│   ├── Harmony.Tests/          # Unit tests
│   │   ├── Domain/             # Domain tests
│   │   ├── ApplicationCore/    # Application tests
│   │   └── Web/                # Web-layer tests (services)
│   └── Harmony.E2ETests/       # E2E tests (Playwright)
│       └── Infrastructure/     # Test fixtures & factories
└── requirements/               # Project requirements
```

## 🎨 Design Principles

### Clean Architecture
- **Dependency Rule**: Dependencies point inwards
- **Separation of Concerns**: Each layer has a clear responsibility
- **Testability**: All layers are unit-testable

### CQRS (Command Query Responsibility Segregation)
- **Commands**: For write operations (CREATE, UPDATE, DELETE)
- **Queries**: For read operations (SELECT)
- **Handlers**: Separation of business logic per use case

### Domain-Driven Design
- **Value Objects**: For type-safe identifiers and validations
- **Entities**: For business objects with identity
- **Repositories**: For data access abstraction

## 🔧 Configuration

### Database Configuration
The application stores its configuration in a file named `harmony.settings.json`. The location of this file depends on where the application is installed:

- **User Installation**: `%APPDATA%\Harmony\harmony.settings.json`
- **System Installation**: `%PROGRAMDATA%\Harmony\harmony.settings.json`

The database location itself is also stored in this file under the `DatabaseDirectory` property. The database file is named `harmony.db`.

### Logging
Standard logging configuration in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 📋 User Guide

1. **Start the application**: Navigate to `https://localhost:5001`
2. **Manage persons**: Go to "Persons" in the menu
3. **Manage groups**: Go to "Groups" in the menu
4. **Add person**: Click "New Person"
5. **Create group**: Click "New Group"
6. **Manage membership**: Use the "Members" button for groups

## 🧪 Testing

The project includes comprehensive tests:

### Unit Tests (`Harmony.Tests`)
- **Domain Tests**: Value objects and entities
- **Application Tests**: Command and query handlers
- Uses xUnit and NSubstitute for mocking

### E2E Tests (`Harmony.E2ETests`)
- **Browser Tests**: Testing the full user flow in a real browser
- Uses Microsoft Playwright for browser automation
- Each test class shares a single running application instance (for faster startup) while each test gets an isolated SQLite database (automatically cleaned up)
- Tests run headless (default) or with visible browser (for debugging)

## 📄 License

This project is licensed under the Apache License, Version 2.0. See the [LICENSE](LICENSE) file for more information.

Copyright 2025 Mark van de Veerdonk

## 🤝 Contributing

Contributions are welcome! Ensure all tests pass and follow the coding standards.

## 🚢 Release Procedure

### Version Management

Version numbers follow the format `MAJOR.MINOR.PATCH` (e.g., `1.2.0`). The version number is managed in one central place:

- **`Directory.Build.props`** — contains `<Version>`, `<AssemblyVersion>`, and `<FileVersion>`. This is the only place that needs to be modified for a new version.

```xml
<Version>1.2.0</Version>
<AssemblyVersion>1.2.0.0</AssemblyVersion>
<FileVersion>1.2.0.0</FileVersion>
```

The build-installer script automatically reads the version number from this file and uses it for the file names of the installer and the zip archive.

### Creating a Release

1. **Update version number** in `Directory.Build.props`:
   ```xml
   <Version>X.Y.Z</Version>
   <AssemblyVersion>X.Y.Z.0</AssemblyVersion>
   <FileVersion>X.Y.Z.0</FileVersion>
   ```

2. **Update release notes** in `RELEASE_NOTES.md` — add a new block at the top of the file with the changes for this version.

3. **Pass all tests**:
   ```powershell
   dotnet test
   ```

4. **Commit changes**:
   ```powershell
   git add .
   git commit -m "Release v X.Y.Z"
   ```

5. **Build installer** from the `installer/` directory:
   ```powershell
   cd installer
   .\build-installer.ps1
   ```

   The script automatically performs the following steps:
   - Publishes `Harmony.Web` as a standalone Windows x64 application
   - Builds the NSIS installer: `Harmony-Setup-X.Y.Z.exe`
   - Creates a zip archive: `Harmony-X.Y.Z.zip`
   - Creates a git tag: `vX.Y.Z`

6. **Push tag to remote**:
   ```powershell
   git push origin vX.Y.Z
   ```

### Installer Requirements

- [NSIS](https://nsis.sourceforge.io/Download) must be installed and `makensis` must be available in the PATH.
- The installer and the zip archive are created in the `installer/` folder.
