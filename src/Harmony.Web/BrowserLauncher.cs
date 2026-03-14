namespace Harmony.Web;

using System.Diagnostics;

internal static class BrowserLauncher
{
    internal static void LaunchAfterStartup(WebApplication app)
    {
        var url = ResolveUrl(app);
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open browser: {ex.Message}");
            }
        });
    }

    private static string ResolveUrl(WebApplication app)
    {
        string url;

        if (app.Urls.Count > 0)
        {
            url = app.Urls.FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                ?? app.Urls.First();
        }
        else
        {
            var configured = app.Configuration["Urls"] ?? app.Configuration["ASPNETCORE_URLS"];
            if (!string.IsNullOrEmpty(configured))
            {
                var urlList = configured.Split(';', StringSplitOptions.RemoveEmptyEntries);
                url = urlList.FirstOrDefault(u => u.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase))?.Trim()
                    ?? urlList.First().Trim();
            }
            else
            {
                url = "http://localhost:5000";
            }
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "http://" + url;

        return url;
    }
}
