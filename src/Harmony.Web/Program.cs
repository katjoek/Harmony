using Harmony.Web;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.AddBlazorInfrastructure();
builder.Services.AddApplicationServices();

var app = builder.Build();

await app.ConfigureListenAddressAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapStaticAssets();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await DatabaseStartup.RunAsync(app);

#if DEBUG
if (await SeedCommandRunner.TryRunAsync(app, args)) return;
#endif

#if RELEASE
BrowserLauncher.LaunchAfterStartup(app);
#endif

app.Run();

// Partial class declaration to make Program accessible for WebApplicationFactory in tests
public partial class Program { }
