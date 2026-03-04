# Harmony - (Kerk-)gemeenschap Beheer

Een moderne webapplicatie voor het beheren van personen en groepen binnen een (kerk-)gemeenschap, gebouwd met Clean Architecture en CQRS principes.

## 🏗️ Architectuur

Het project volgt Clean Architecture principes met de volgende lagen:

- **Domain**: Kernlogica en business rules
- **ApplicationCore**: Use cases en CQRS implementatie
- **Infrastructure**: Data access en externe services
- **Web**: Blazor Server UI
- **Tests**: Unit tests voor alle lagen

## 🚀 Technologieën

- **.NET 10.0**: Moderne framework voor C#
- **Blazor Server**: Interactieve webapplicatie
- **BootstrapBlazor**: UI componenten
- **Entity Framework Core**: ORM voor database toegang
- **SQLite**: Embedded database
- **LiteBus**: CQRS implementatie
- **xUnit**: Unit testing framework
- **Playwright**: E2E browser testing

## 🎯 Functionaliteiten

### Personen Beheer
- ✅ Personen toevoegen, bewerken en verwijderen
- ✅ Voornaam (verplicht), tussenvoegsel, achternaam
- ✅ Geboortedatum, adres, telefoon, e-mail
- ✅ Lidmaatschap van groepen beheren
- ✅ Cascading delete bij verwijdering

### Groepen Beheer
- ✅ Groepen aanmaken, bewerken en verwijderen
- ✅ Unieke groepsnaam (verplicht)
- ✅ Coördinator toewijzen (optioneel)
- ✅ Leden toevoegen en verwijderen
- ✅ Non-cascading delete bij verwijdering

### Gebruikersinterface
- ✅ Nederlandse interface
- ✅ Professioneel en gebruiksvriendelijk design
- ✅ Bevestigingsdialogen bij verwijdering
- ✅ Responsief design voor verschillende schermformaten

## 🛠️ Installatie en Gebruik

### Vereisten
- .NET 10.0 SDK
- Visual Studio 2022 of VS Code

### Installatie
```bash
# Clone de repository
git clone [repository-url]
cd Harmony2

# Restore packages
dotnet restore

# Build de solution
dotnet build

# Run de applicatie
cd Harmony.Web
dotnet run
```

### Database
De applicatie gebruikt SQLite met automatische database creatie. De database wordt aangemaakt bij de eerste start.

### Tests uitvoeren

#### Unit Tests
```bash
# Run unit tests
dotnet test test/Harmony.Tests

# Run met coverage
dotnet test test/Harmony.Tests --collect:"XPlat Code Coverage"
```

#### E2E Tests (Playwright)
De E2E tests gebruiken Microsoft Playwright om de applicatie in een echte browser te testen.

**Eerste keer setup:**
```bash
# Build het E2E test project
dotnet build test/Harmony.E2ETests

# Installeer Playwright browsers (alleen eerste keer nodig)
pwsh test/Harmony.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

**Tests uitvoeren:**
```bash
# Run E2E tests
dotnet test test/Harmony.E2ETests

# Run met gedetailleerde output
dotnet test test/Harmony.E2ETests --logger "console;verbosity=detailed"
```

**Debugging:** Voor visueel debuggen, wijzig `Headless = true` naar `Headless = false` in `test/Harmony.E2ETests/Infrastructure/PlaywrightFixture.cs`.

#### Alle tests
```bash
# Run alle tests (unit + E2E)
dotnet test
```

## 📁 Project Structuur

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
│   ├── Shared/                 # Shared components
│   └── wwwroot/                # Static files
├── test/
│   ├── Harmony.Tests/          # Unit tests
│   │   ├── Domain/             # Domain tests
│   │   └── ApplicationCore/    # Application tests
│   └── Harmony.E2ETests/       # E2E tests (Playwright)
│       └── Infrastructure/     # Test fixtures & factories
└── requirements/               # Project requirements
```

## 🎨 Design Principes

### Clean Architecture
- **Dependency Rule**: Dependencies wijzen naar binnen
- **Separation of Concerns**: Elke laag heeft een duidelijke verantwoordelijkheid
- **Testability**: Alle lagen zijn unit testbaar

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Voor write operations (CREATE, UPDATE, DELETE)
- **Queries**: Voor read operations (SELECT)
- **Handlers**: Scheiden van business logic per use case

### Domain-Driven Design
- **Value Objects**: Voor type-safe identifiers en validaties
- **Entities**: Voor business objecten met identiteit
- **Repositories**: Voor data toegang abstractie

## 🔧 Configuratie

### Connectionstring
De database connectionstring kan geconfigureerd worden in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=harmony.db"
  }
}
```

### Logging
Standaard logging configuratie in `appsettings.json`:

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

## 📋 Gebruiksaanwijzing

1. **Start de applicatie**: Navigeer naar `https://localhost:5001`
2. **Personen beheren**: Ga naar "Personen" in het menu
3. **Groepen beheren**: Ga naar "Groepen" in het menu
4. **Persoon toevoegen**: Klik op "Nieuwe Persoon"
5. **Groep aanmaken**: Klik op "Nieuwe Groep"
6. **Lidmaatschap beheren**: Gebruik de "Leden" knop bij groepen

## 🧪 Testing

Het project bevat uitgebreide tests:

### Unit Tests (`Harmony.Tests`)
- **Domain Tests**: Value objects en entities
- **Application Tests**: Command en query handlers
- Gebruikt xUnit en NSubstitute voor mocking

### E2E Tests (`Harmony.E2ETests`)
- **Browser Tests**: Testen van de volledige gebruikersstroom in een echte browser
- Gebruikt Microsoft Playwright voor browser automatisering
- Elke test krijgt een geïsoleerde SQLite database (automatisch opgeruimd)
- Tests draaien headless (standaard) of met zichtbare browser (voor debugging)

## 📄 Licentie

Dit project is gelicenseerd onder de Apache License, Version 2.0. Zie het [LICENSE](LICENSE) bestand voor meer informatie.

Copyright 2025 Mark van de Veerdonk

## 🤝 Bijdragen

Bijdragen zijn welkom! Zorg ervoor dat alle tests slagen en volg de coding standards.

## 🚢 Release Procedure

### Versiebeheer

Versienummers volgen het formaat `MAJOR.MINOR.PATCH` (bijv. `1.2.0`). Het versienummer wordt op één centrale plek beheerd:

- **`Directory.Build.props`** — bevat `<Version>`, `<AssemblyVersion>` en `<FileVersion>`. Dit is de enige plek die aangepast moet worden voor een nieuwe versie.

```xml
<Version>1.2.0</Version>
<AssemblyVersion>1.2.0.0</AssemblyVersion>
<FileVersion>1.2.0.0</FileVersion>
```

Het build-installer script leest het versienummer automatisch uit dit bestand en gebruikt het voor de bestandsnamen van de installer en het zip-archief.

### Release aanmaken

1. **Versienummer bijwerken** in `Directory.Build.props`:
   ```xml
   <Version>X.Y.Z</Version>
   <AssemblyVersion>X.Y.Z.0</AssemblyVersion>
   <FileVersion>X.Y.Z.0</FileVersion>
   ```

2. **Release notes bijwerken** in `RELEASE_NOTES.md` — voeg een nieuw blok toe bovenaan het bestand met de wijzigingen van deze versie.

3. **Alle tests laten slagen**:
   ```powershell
   dotnet test
   ```

4. **Wijzigingen committen**:
   ```powershell
   git add .
   git commit -m "Release v X.Y.Z"
   ```

5. **Installer bouwen** vanuit de `installer/` map:
   ```powershell
   cd installer
   .\build-installer.ps1
   ```

   Het script voert automatisch de volgende stappen uit:
   - Publiceert `Harmony.Web` als zelfstandige Windows x64 applicatie
   - Bouwt de NSIS installer: `Harmony-Setup-X.Y.Z.exe`
   - Maakt een zip-archief: `Harmony-X.Y.Z.zip`
   - Maakt een git-tag aan: `vX.Y.Z`

6. **Tag naar remote pushen**:
   ```powershell
   git push origin vX.Y.Z
   ```

### Vereisten voor de installer

- [NSIS](https://nsis.sourceforge.io/Download) moet geïnstalleerd zijn en `makensis` moet beschikbaar zijn in het PATH.
- De installer en het zip-archief worden aangemaakt in de `installer/` map.
