# Harmony2 — Claude Instructions

## Project Overview
Harmony is a Blazor Server app for managing church community members and groups. It targets .NET 10, uses SQLite via EF Core, and LiteBus for CQRS.

**Solution layout:**
- `src/Harmony.Domain` — Entities, value objects. No async, no framework dependencies.
- `src/Harmony.ApplicationCore` — Commands, queries, handlers, DTOs, repository interfaces. Depends only on Domain.
- `src/Harmony.Infrastructure` — EF Core DbContext, repositories, services. Depends on ApplicationCore.
- `src/Harmony.Web` — Blazor Server UI, report generation. References ApplicationCore and Infrastructure.
- `test/Harmony.Tests` — Unit tests (xUnit + NSubstitute).
- `test/Harmony.E2ETests` — Playwright end-to-end tests.
- `tools/` — Standalone utilities (seeder, import tool).

## Architecture Rules (always apply)
- **Clean Architecture**: all dependencies point inward — Infrastructure and Web depend on ApplicationCore, never the reverse.
- **CQRS via LiteBus**: write operations are Commands, read operations are Queries. Command handlers must not return data. Query handlers must not mutate state.
- For every new feature: define the Command or Query first, then the handler, then the repository interface if needed.
- No business logic in Web layer (Blazor pages/services are orchestrators only).
- No async code or framework dependencies in Domain.

## Packages in Use
- **LiteBus** (`LiteBus.Commands`, `LiteBus.Queries`, `LiteBus.Extensions.MicrosoftDependencyInjection`) — CQRS messaging
- **EF Core 10 + SQLite** — data access, code-first migrations
- **iText7** — PDF generation
- **EPPlus** — Excel generation
- **xUnit** — testing framework
- **NSubstitute** — mocking in unit tests
- **Verify** — snapshot assertions when verifying more than 5 properties
- **BootstrapBlazor** — UI components
- Package versions are managed centrally in `Directory.Packages.props` — do not add `Version` attributes in individual `.csproj` files.

## Development Environment
- Windows machine — use Windows-compatible shell commands.
- `dotnet` CLI is available.

## C# Coding Style

### Types
- Prefer `record` for data types (immutable, value semantics).
- Make classes `sealed` by default; only unseal when inheritance is explicitly designed for.
- Use value objects to avoid primitive obsession (see existing `PersonName`, `EmailAddress`, `PhoneNumber`).
- Use `ImmutableList<T>` / `ImmutableDictionary<K,V>` inside records.

### Code organisation
- File-scoped namespaces. Place `using` statements **below** the namespace declaration.
- Separate state from behaviour — put operations in `static` methods or extension methods, not mixed into data records.
- Use extension methods for domain-specific operations on types you own.

### Functional patterns
- Prefer pattern matching (`switch` expressions) over chains of `if` statements.
- Prefer pure methods (no hidden side effects or hidden dependencies).
- Prefer range indexers (`items[^1]`, `items[..3]`) over equivalent LINQ.

### Naming & clarity
- Use full, descriptive names — no abbreviations (`cancellationToken`, not `ct`).
- Always use `nameof()` for parameter names in exceptions and logging.

### Nullability
- Nullable reference types are enabled with `TreatWarningsAsErrors` — keep it clean.
- Mark nullable fields/parameters explicitly with `?`.
- Use `ArgumentNullException.ThrowIfNull` for guard clauses.

### Error handling
- Use `Result<T>` for expected failures at application boundaries.
- Use exceptions for truly exceptional/programming-error cases.
- Never swallow exceptions silently.
- Remove empty `catch { throw; }` blocks — they add noise without value.
- Use `ILogger<T>` for diagnostics — no `Console.WriteLine` in production code.

### Async
- Always propagate `CancellationToken` through async call chains.
- Never use `Task.Result`, `Task.Wait`, or `async void` (except Blazor event handlers).
- Prefer `async/await` over `.ContinueWith`.
- Use `ValueTask` for zero-allocation hot paths.

## Testing Rules

### Frameworks & tools
- **xUnit** — only test framework.
- **NSubstitute** — mocking/faking interfaces.
- **Verify** — snapshot assertions when verifying more than 5 properties.
- No FluentAssertions or other assertion libraries — use xUnit's built-in `Assert.*`.

### Test structure
- Follow **Arrange / Act / Assert** with clear section comments.
- One behaviour per test — name as `Method_Condition_ExpectedResult`.
- Prefer `[Theory]` + `TheoryData<>` / `[InlineData]` over multiple near-identical `[Fact]`s.
- Keep test data local: use `static` factory methods inside the test class, not shared globals.
- Test classes are `sealed`.

### Isolation
- Each test must be fully independent — no shared mutable state between tests.
- For integration tests, use isolated SQLite databases (see `HarmonyWebApplicationFactory`).

### What to test
- Domain logic (value objects, entities) — pure unit tests, no mocks needed.
- Command/query handlers — unit tests with NSubstitute fakes for repositories.
- Services with side effects — unit tests verifying calls on fakes.
- E2E flows — Playwright tests in `Harmony.E2ETests`.
