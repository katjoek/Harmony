# Release Notes

## Version 1.2.1

Changes since v1.2.0

### Improvements

- **Group report filename**: The group name is now included in the filename of group reports.

### Technical

- **Program.cs split into focused startup classes**: Startup logic has been extracted from `Program.cs` into dedicated classes — `BrowserLauncher`, `DatabaseStartup`, `SeedCommandRunner`, `ServiceCollectionExtensions`, and `WebApplicationExtensions` — making each concern easier to find and test independently.
- **Injectable report filename service**: Report filename logic has been extracted from `ReportService` into a dedicated `IReportFileNameBuilder` interface and `ReportFileNameBuilder` implementation, making it independently testable and replaceable.
- **Razor file simplification**: Pages and shared components (`Groepen`, `Personen`, `Instellingen`, `Rapporten`, `DualListMembershipModal`, `BackupCard`, `GroupFormModal`, `PersonFormModal`, `SearchBar`) have been simplified and reduced in size.
- **ReportService simplified**: Internal implementation of `ReportService` cleaned up.
- **E2E test startup performance**: The app server and Playwright browser are now shared across all E2E test classes, significantly reducing total test run time.
- **E2E test data seeding via database**: Test data is now seeded directly through the database instead of via the UI, making tests faster and more reliable.
- **E2E test coverage expanded**: Added tests for group creation (`GroupCreationTests`), group membership modal (`GroupMembershipModalTests`), and setting group coordinator (`GroupCoordinatorTests`).
- **Dual-list JS helpers extracted**: Common Playwright helper methods for the dual-list membership modal have been extracted into `DualListPageExtensions` extension methods on `IPage`.
- **Unit tests expanded**: Added unit tests for `ReportService` and `ReportFileNameBuilder`.

---

## Version 1.2.0

Changes since v1.1.1

### Improvements
- **Copy email addresses**: Added the ability to copy email addresses of group members to the clipboard with a single button click on the groups page.
- **Report preview removed**: It added no value and the preview was not always accurate.

### Technical

- **Upgrade to .NET 10**: The application and all projects have been migrated from .NET 9 to .NET 10.
- **NuGet package updates**: All NuGet packages updated to the latest stable versions.
  - Entity Framework Core: 9.0.1 → 10.0.3
  - Microsoft.Extensions.*: 9.0.x → 10.0.3
  - iText7: 8.0.5 → 9.5.0
  - EPPlus: 7.2.2 → 8.4.2
  - Microsoft.Playwright: 1.49.0 → 1.58.0
- **iText7 9.x compatibility**: Replaced `SetBold()` with `SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))`.
- **EPPlus 8.x compatibility**: Replaced `ExcelPackage.LicenseContext` with `ExcelPackage.License.SetNonCommercialPersonal()`.
- **Static web assets**: Added `UseStaticWebAssets()` so `blazor.server.js` loads correctly in production.
- **HTTPS redirect**: HTTPS redirect is now skipped in the test environment.

### Documentation

- **English docs**: the README.md and RELEASE_NOTES.md files have been translated into English.

---

## Version 1.1.1

Changes since v1.1.0

### Improvements

- **Birthday report layout**: Date of birth is now the first column in birthday reports.
- **Generation feedback**: A loading indicator is now shown during report generation.
- **Name display**: When sorting by surname, the name is displayed as "Surname, First Name Prefix".
- **Birthday report group selection**: Selected group for birthday reports is now remembered between sessions (group reports always start without a selection).
- **Birthday report filename**: For birthday reports, the group name is now included in the filename when a group is selected.

### Bugfixes

- **Configuration storage**: Fixed storing the selected group for birthday reports by correctly handling database migrations.
- **Settings file location**: Settings file is now stored in AppData or ProgramData (depending on installation location) instead of the application folder, ensuring the app works correctly when installed in `C:\Program Files`.

### Technical

- Added E2E tests with Microsoft Playwright for creating persons.
- Database seeding is now only available in DEBUG builds.
- Improved handling of database migrations for existing databases.
- Installer script now also creates a zip file of the publish folder.
- Installer script deletes the publish folder before performing the build.

---

## Version 1.1.0

Changes since v1.0.1

### New Functionality

- **Birthday report per group**: User can select a group to filter birthdays.
- **Network configuration**: User can choose to listen on localhost or 0.0.0.0.
- **Installer**: Added installer for easier distribution.
- **Auto-start browser**: In release mode, the application automatically starts the browser.
- **Application logo**: Introduced Harmony logo.

### Improvements

- **Reports page**: Works immediately after startup; reports page has been made more organized.
- **Live preview**: Changing column selection is immediately reflected in the preview.
- **Groups column**: Added column for number of groups on the Persons page.
- **Search focus**: Search field automatically gets focus when navigating to the persons or groups page.
- **F3 shortcut**: F3 key places the cursor in the search field.
- **Sorting**: Case-insensitive sorting for persons and group names.
- **Home page**: Improved content of the home page.
- **UI colors**: Improved color scheme.
- **Textual improvements**: Various textual improvements throughout the application.

### Bugfixes

- **Membership modal**: Arrow buttons didn't work when editing memberships via the groups page. Now the DualListMembershipModal component is used, just like on the persons page.
- **Data integrity**: Memberships are now deleted when a person or group is deleted.
- **Clean up orphaned records**: On startup, membership records for non-existent persons or groups are removed.

### Technical

- Added version number to the filename when creating the installer.
- Improved release procedure.
- Logging to log file instead of console.
- Changed/specified the code license.
- GIT tag is created after successfully building the installer.
