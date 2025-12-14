using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services;
using Harmony.Web.Commands;
using Harmony.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

const int maxFileSize = 10 * 1024 * 1024;

// Configure file upload size limits (for database backup uploads)
// Default is 500 KB, increase to 100 MB to accommodate database backups
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxFileSize;
    options.ValueLengthLimit = maxFileSize;
    options.ValueCountLimit = 10; // Maximum number of form values
});

// Configure Kestrel server limits for larger request bodies
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxFileSize;
});

// Configure Blazor Server hub options for larger messages
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = maxFileSize;
});

// Register settings service as singleton (file-based, shared across requests)
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Register connection string provider
builder.Services.AddScoped<IDatabaseConnectionStringProvider, Harmony.Infrastructure.Services.DatabaseConnectionStringProvider>();

// Add Entity Framework with dynamic connection string from settings (resolved per request)
// Register DbContext using a factory pattern to defer connection string resolution
// until the DbContext is actually created (prevents database access if directory not configured)
builder.Services.AddScoped<HarmonyDbContext>(serviceProvider =>
{
    var connectionStringProvider = serviceProvider.GetRequiredService<IDatabaseConnectionStringProvider>();
    // During service registration, we can use GetAwaiter().GetResult() safely
    // as we're not in a Blazor Server synchronization context
    var connectionString = connectionStringProvider.GetConnectionStringAsync().GetAwaiter().GetResult();
    
    var optionsBuilder = new DbContextOptionsBuilder<HarmonyDbContext>();
    optionsBuilder.UseSqlite(connectionString);
    
    return new HarmonyDbContext(optionsBuilder.Options);
});

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Harmony.ApplicationCore.Commands.Persons.CreatePersonCommand).Assembly);
});

// Add repositories and services
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IReportService, ReportService>();
 builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<SeedDataCommand>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure database is created (only if directory is configured)
using (var scope = app.Services.CreateScope())
{
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var isConfigured = await settingsService.IsDatabaseDirectoryConfiguredAsync();
    
    if (isConfigured)
    {
        var context = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
        context.Database.EnsureCreated();
    }
}

// Check for seed command line argument
if (args.Length > 0 && args[0] == "seed")
{
    using var scope = app.Services.CreateScope();
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var isConfigured = await settingsService.IsDatabaseDirectoryConfiguredAsync();
    
    if (!isConfigured)
    {
        Console.WriteLine("ERROR: Database directory is not configured. Please configure the database directory first.");
        Console.WriteLine("Run the application and configure the database directory through the web interface.");
        return;
    }
    
    Console.WriteLine("Seeding database...");
    var seedCommand = scope.ServiceProvider.GetRequiredService<SeedDataCommand>();
    await seedCommand.ExecuteAsync();
    Console.WriteLine("Seeding completed. Exiting application.");
    return;
}

app.Run();
