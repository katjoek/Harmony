# Release Notes

## Versie 1.1.1

Wijzigingen sinds v1.1.0

### Verbeteringen

- **Verjaardagsrapport layout**: Geboortedatum is nu de eerste kolom in verjaardagsrapporten
- **Feedback bij genereren**: Laadindicator wordt nu getoond tijdens het genereren van rapporten
- **Naamweergave**: Bij sorteren op achternaam wordt de naam weergegeven als "Achternaam, Voornaam Tussenvoegsel"
- **Groepsselectie verjaardagsrapport**: Geselecteerde groep voor verjaardagsrapporten wordt nu onthouden tussen sessies (groepsrapporten starten altijd zonder selectie)

### Bugfixes

- **Configuratieopslag**: Opslaan van geselecteerde groep voor verjaardagsrapporten gerepareerd door databasemigraties correct af te handelen

### Technisch

- E2E-tests toegevoegd met Microsoft Playwright voor het aanmaken van personen
- Database seeding is nu alleen beschikbaar in DEBUG-builds
- Verbeterde afhandeling van databasemigraties voor bestaande databases

---

## Versie 1.1.0

Wijzigingen sinds v1.0.1

### Nieuwe functionaliteit

- **Verjaardagsrapport per groep**: Gebruiker kan een groep selecteren om verjaardagen uit te filteren
- **Netwerkconfiguratie**: Gebruiker kan kiezen om te luisteren op localhost of 0.0.0.0
- **Installer**: Installer toegevoegd voor eenvoudigere distributie
- **Browser automatisch starten**: In release-modus start de applicatie automatisch de browser
- **Applicatielogo**: Logo voor Harmony geïntroduceerd

### Verbeteringen

- **Rapportenpagina**: Werkt direct na opstarten; rapportenpagina is overzichtelijker gemaakt
- **Live voorbeeld**: Wijzigen van kolomselectie wordt direct weergegeven in het voorbeeld
- **Groepenkolom**: Kolom toegevoegd voor aantal groepen op de Personenpagina
- **Zoekfocus**: Zoekveld krijgt automatisch focus bij navigeren naar personen- of groepenpagina
- **F3 sneltoets**: F3-toets zet de cursor in het zoekveld
- **Sortering**: Hoofdletterongevoelig sorteren van personen en groepsnamen
- **Homepagina**: Inhoud van de homepagina verbeterd
- **UI kleuren**: Kleurenschema verbeterd
- **Tekstuele verbeteringen**: Diverse tekstuele verbeteringen door de hele applicatie

### Bugfixes

- **Lidmaatschapsmodal**: Pijlknoppen werkten niet bij bewerken van lidmaatschappen via groepenpagina. Nu wordt het DualListMembershipModal-component gebruikt, net als op de personenpagina
- **Data-integriteit**: Lidmaatschappen worden nu verwijderd wanneer een persoon of groep wordt verwijderd
- **Opschonen verweesde records**: Bij opstarten worden lidmaatschapsrecords voor niet-bestaande personen of groepen verwijderd

### Technisch

- Bij het maken van de installer wordt nu het versienummer toegevoegd aan de bestandsnaam
- Verbeterde release-procedure
- Schrijven naar logbestand in plaats van console
- Licentie van de code gewijzigd/gespecificeerd
- GIT-tag wordt aangemaakt na succesvol maken van installer
