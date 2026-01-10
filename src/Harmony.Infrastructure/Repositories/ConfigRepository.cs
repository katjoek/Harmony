using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories;

public sealed class ConfigRepository : IConfigRepository
{
    private readonly HarmonyDbContext _context;

    public ConfigRepository(HarmonyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var config = await _context.Configs
            .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);

        return config?.Value;
    }

    public async Task SetValueAsync(string key, string? value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var config = await _context.Configs
            .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);

        if (config == null)
        {
            config = new Config(key, value);
            _context.Configs.Add(config);
        }
        else
        {
            config.UpdateValue(value);
            _context.Configs.Update(config);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
