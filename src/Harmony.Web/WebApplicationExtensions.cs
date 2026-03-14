namespace Harmony.Web;

using Harmony.ApplicationCore.Interfaces;

internal static class WebApplicationExtensions
{
    internal static async Task ConfigureListenAddressAsync(this WebApplication app)
    {
        var settingsService = app.Services.GetRequiredService<ISettingsService>();
        if (!await settingsService.GetListenOnAllInterfacesAsync()) return;

        if (app.Urls.Count > 0)
        {
            var modified = app.Urls
                .Select(url =>
                    url.Contains("://localhost", StringComparison.OrdinalIgnoreCase) ||
                    url.Contains("://127.0.0.1", StringComparison.OrdinalIgnoreCase)
                        ? url.Replace("localhost", "0.0.0.0", StringComparison.OrdinalIgnoreCase)
                             .Replace("127.0.0.1", "0.0.0.0", StringComparison.OrdinalIgnoreCase)
                        : url)
                .ToList();

            app.Urls.Clear();
            foreach (var url in modified)
                app.Urls.Add(url);
        }
        else
        {
            app.Urls.Add("http://0.0.0.0:5000");
        }
    }
}
