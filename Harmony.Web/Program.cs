using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Web.Commands;
using Harmony.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework
builder.Services.AddDbContext<HarmonyDbContext>(options =>
{
    var raw = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=%APPDATA%\\Harmony\\harmony.db";
    var expanded = Environment.ExpandEnvironmentVariables(raw);

    // Ensure directory exists for the SQLite file
    var dataSourcePrefix = "Data Source=";
    var idx = expanded.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        var pathPart = expanded.Substring(idx + dataSourcePrefix.Length).Trim();
        var directory = Path.GetDirectoryName(pathPart);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    options.UseSqlite(expanded);
});

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Harmony.ApplicationCore.Commands.Persons.CreatePersonCommand).Assembly);
});

// Add repositories and services
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMembershipService, Harmony.Infrastructure.Services.MembershipService>();
builder.Services.AddScoped<IReportService, ReportService>();
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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
    context.Database.EnsureCreated();
}

// Check for seed command line argument
if (args.Length > 0 && args[0] == "seed")
{
    Console.WriteLine("Seeding database...");
    using var scope = app.Services.CreateScope();
    var seedCommand = scope.ServiceProvider.GetRequiredService<SeedDataCommand>();
    await seedCommand.ExecuteAsync();
    Console.WriteLine("Seeding completed. Exiting application.");
    return;
}

app.Run();
