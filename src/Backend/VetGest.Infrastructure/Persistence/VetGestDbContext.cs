using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetGest.Domain.Entities;
using VetGest.Infrastructure.Identity;
using VetGest.Infrastructure.Persistence.EntityConfigurations;

namespace VetGest.Infrastructure.Persistence;

/// <summary>
/// VetGest database context with Identity integration and global query filters.
/// Supports multi-tenant data isolation at the database level via shadow properties.
/// </summary>
public class VetGestDbContext : IdentityDbContext<ApplicationUser, IdentityRole<string>, string>
{
    public VetGestDbContext(DbContextOptions<VetGestDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Shadow property for the owning user ID (tenant isolation).
    /// Applied via global query filters to all tenant-specific entities.
    /// </summary>
    private static readonly string OwnerId = nameof(OwnerId);

    /// <summary>
    /// Shadow property for entity creation timestamp (audit trail).
    /// </summary>
    private static readonly string CreatedAt = nameof(CreatedAt);

    /// <summary>
    /// Shadow property for entity modification timestamp (audit trail).
    /// </summary>
    private static readonly string ModifiedAt = nameof(ModifiedAt);

    /// <summary>
    /// Pet entities (the root aggregate).
    /// Tracks pets belonging to a single owner.
    /// </summary>
    public DbSet<Pet> Pets { get; set; } = null!;

    /// <summary>
    /// Pregnancy entities.
    /// Tracks pregnancy records for each pet.
    /// </summary>
    public DbSet<Pregnancy> Pregnancies { get; set; } = null!;

    /// <summary>
    /// PregnancyDiary entries.
    /// Tracks daily observations during pregnancy.
    /// </summary>
    public DbSet<PregnancyDiaryEntry> PregnancyDiaryEntries { get; set; } = null!;

    /// <summary>
    /// Phase entities.
    /// Reference data describing the pregnancy phases (Initial, Intermediate, Growth, Final, Birth).
    /// </summary>
    public DbSet<Phase> Phases { get; set; } = null!;

    /// <summary>
    /// AlertRule entities.
    /// Reference data defining alert rules for pregnancy monitoring.
    /// </summary>
    public DbSet<AlertRule> AlertRules { get; set; } = null!;

    /// <summary>
    /// VetConnection entities.
    /// Manages vet-tutor connections to pregnancies via invitation codes.
    /// </summary>
    public DbSet<VetConnection> VetConnections { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables with appropriate schema
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<string>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        // Configure ApplicationUser extensions
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.FullName)
            .HasMaxLength(256);

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Apply entity configurations from EntityConfigurations folder
        modelBuilder.ApplyConfiguration(new PetConfiguration());
        modelBuilder.ApplyConfiguration(new PregnancyConfiguration());
        modelBuilder.ApplyConfiguration(new PregnancyDiaryEntryConfiguration());
        modelBuilder.ApplyConfiguration(new PhaseConfiguration());
        modelBuilder.ApplyConfiguration(new AlertRuleConfiguration());
        modelBuilder.ApplyConfiguration(new VetConnectionConfiguration());
    }

    /// <summary>
    /// Override SaveChanges to update audit fields automatically.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to update audit fields automatically.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update shadow properties for audit tracking.
    /// CreatedAt is set only on Add; ModifiedAt is set on Add and Modify.
    /// </summary>
    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is ApplicationUser)
            {
                // Skip automatic audit update for ApplicationUser (has explicit properties)
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty(CreatedAt) is not null)
                    entry.Property(CreatedAt).CurrentValue = now;

                if (entry.Metadata.FindProperty(ModifiedAt) is not null)
                    entry.Property(ModifiedAt).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty(ModifiedAt) is not null)
                    entry.Property(ModifiedAt).CurrentValue = now;
            }
        }
    }
}
