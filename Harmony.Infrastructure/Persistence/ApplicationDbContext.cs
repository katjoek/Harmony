using Harmony.Application.Interfaces;
using Harmony.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Person> People { get; set; }
    public DbSet<Group> Groups { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
    {
        return await Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Person entity
        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(p => p.FirstName).HasMaxLength(20);
            entity.Property(p => p.MiddleName).HasMaxLength(10);
            entity.Property(p => p.LastName).HasMaxLength(30);
            entity.Property(p => p.StreetAndHouseNumber).HasMaxLength(80);
            entity.Property(p => p.City).HasMaxLength(30);
            entity.Property(p => p.ZipCode).HasMaxLength(7);
            entity.Property(p => p.PhoneNumber).HasMaxLength(12);
            entity.Property(p => p.EmailAddress).HasMaxLength(128);
        });

        // Configure Group entity
        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(g => g.Name).HasMaxLength(50);

            // Configure the Coordinator relationship
            entity.HasOne(g => g.Coordinator)
                .WithMany()
                .HasForeignKey(g => g.CoordinatorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure the many-to-many relationship between Person and Group
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithMany(p => p.Memberships)
            .UsingEntity<Dictionary<string, object>>(
                "GroupMembers",
                j => j
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey("PersonId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Group>()
                    .WithMany()
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("GroupId", "PersonId");
                    j.ToTable("GroupMembers");
                });
    }
}
