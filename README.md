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

- **.NET 9.0**: Moderne framework voor C#
- **Blazor Server**: Interactieve webapplicatie
- **BootstrapBlazor**: UI componenten
- **Entity Framework Core**: ORM voor database toegang
- **SQLite**: Embedded database
- **LiteBus**: CQRS implementatie
- **xUnit**: Unit testing framework

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
- .NET 9.0 SDK
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
```bash
# Run alle tests
dotnet test

# Run tests met coverage
dotnet test --collect:"XPlat Code Coverage"
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
├── Harmony.Tests/              # Test project
│   ├── Domain/                 # Domain tests
│   └── ApplicationCore/        # Application tests
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

Het project bevat uitgebreide unit tests:

- **Domain Tests**: Value objects en entities
- **Application Tests**: Command en query handlers
- **Integration Tests**: End-to-end scenarios

## 📄 Licentie

Dit project is gelicenseerd onder de Apache License, Version 2.0. Zie het [LICENSE](LICENSE) bestand voor meer informatie.

Copyright 2025 Mark van de Veerdonk

## 🤝 Bijdragen

Bijdragen zijn welkom! Zorg ervoor dat alle tests slagen en volg de coding standards.
