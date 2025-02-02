using Harmony.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Person> People { get; set; }
    DbSet<Group> Groups { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
    // Optional: If you need to work with raw SQL queries
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);
    Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default);
        
    // Optional: If you need to work with transactions
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}