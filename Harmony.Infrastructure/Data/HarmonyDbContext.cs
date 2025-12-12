using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Data;

public sealed class HarmonyDbContext : DbContext
{
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<PersonGroupMembership> PersonGroupMemberships { get; set; } = null!;

    public HarmonyDbContext(DbContextOptions<HarmonyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePersonEntity(modelBuilder);
        ConfigureGroupEntity(modelBuilder);
        ConfigureMembershipEntity(modelBuilder);
    }

    private static void ConfigurePersonEntity(ModelBuilder modelBuilder)
    {
        var personBuilder = modelBuilder.Entity<Person>();

        personBuilder.HasKey(p => p.Id);

        personBuilder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new PersonId(value))
            .IsRequired();

        personBuilder.OwnsOne(p => p.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.Prefix)
                .HasMaxLength(50);

            nameBuilder.Property(n => n.Surname)
                .HasMaxLength(100);
        });

        personBuilder.Property(p => p.DateOfBirth);

        personBuilder.OwnsOne(p => p.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasMaxLength(200);

            addressBuilder.Property(a => a.ZipCode)
                .HasMaxLength(20);

            addressBuilder.Property(a => a.City)
                .HasMaxLength(100);
        });
        
        // Configure Address navigation as optional to resolve EF Core warning
        personBuilder.Navigation(p => p.Address)
            .IsRequired(false);

        personBuilder.Property(p => p.PhoneNumber)
            .HasConversion(
                phone => phone != null ? phone.Value : null,
                value => value != null ? new PhoneNumber(value) : null)
            .HasMaxLength(50);

        personBuilder.Property(p => p.EmailAddress)
            .HasConversion(
                email => email != null ? email.Value : null,
                value => value != null ? new EmailAddress(value) : null)
            .HasMaxLength(200);

        // Add indexes for better search performance
        personBuilder.HasIndex(p => p.EmailAddress);

        // For simplicity, we'll handle group membership through a separate join approach
        personBuilder.Ignore(p => p.GroupIds);
    }

    private static void ConfigureGroupEntity(ModelBuilder modelBuilder)
    {
        var groupBuilder = modelBuilder.Entity<Group>();

        groupBuilder.HasKey(g => g.Id);

        groupBuilder.Property(g => g.Id)
            .HasConversion(
                id => id.Value,
                value => new GroupId(value))
            .IsRequired();

        groupBuilder.Property(g => g.Name)
            .HasMaxLength(200)
            .IsRequired();

        groupBuilder.HasIndex(g => g.Name)
            .IsUnique();

        groupBuilder.Property(g => g.CoordinatorId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new PersonId(value.Value) : null);

        // For simplicity, we'll handle group membership through a separate join approach
        groupBuilder.Ignore(g => g.MemberIds);
        groupBuilder.Ignore(g => g.MemberCount);
    }

    private static void ConfigureMembershipEntity(ModelBuilder modelBuilder)
    {
        var membershipBuilder = modelBuilder.Entity<PersonGroupMembership>();

        membershipBuilder.HasKey(m => new { m.PersonId, m.GroupId });

        membershipBuilder.Property(m => m.PersonId)
            .HasConversion(
                id => id.Value,
                value => new PersonId(value));

        membershipBuilder.Property(m => m.GroupId)
            .HasConversion(
                id => id.Value,
                value => new GroupId(value));

        // Add indexes for better query performance
        membershipBuilder.HasIndex(m => m.PersonId);
        membershipBuilder.HasIndex(m => m.GroupId);
    }
}

// Join table entity for many-to-many relationship
public sealed class PersonGroupMembership
{
    public PersonId PersonId { get; set; } = null!;
    public GroupId GroupId { get; set; } = null!;
}
