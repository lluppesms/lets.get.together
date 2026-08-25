//-----------------------------------------------------------------------
// <copyright file="DadABaseDbContext.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// DadABase Database Context
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data;

/// <summary>
/// Represents the Entity Framework database context for DadABase.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DadABaseDbContext"/> class.
/// </remarks>
/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
[ExcludeFromCodeCoverage]
public class DadABaseDbContext(DbContextOptions<DadABaseDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the set of jokes in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="Joke"/> entities.</value>
    public DbSet<Joke>? Jokes { get; set; }

    /// <summary>
    /// Gets or sets the set of joke categories in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeCategory"/> entities.</value>
    public DbSet<JokeCategory>? JokeCategories { get; set; }

    /// <summary>
    /// Gets or sets the set of joke ratings in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeRating"/> entities.</value>
    public DbSet<JokeRating>? JokeRatings { get; set; }

    /// <summary>
    /// Gets or sets the junction dataset representing the many-to-many relationship between jokes and categories.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeJokeCategory"/> entities.</value>
    public DbSet<JokeJokeCategory>? JokeJokeCategories { get; set; }

    /// <summary>
    /// Gets or sets the set of users in the Get Together domain.
    /// </summary>
    public DbSet<User>? Users { get; set; }

    /// <summary>
    /// Gets or sets the set of circles in the Get Together domain.
    /// </summary>
    public DbSet<Circle>? Circles { get; set; }

    /// <summary>
    /// Gets or sets the set of circle memberships in the Get Together domain.
    /// </summary>
    public DbSet<CircleMembership>? CircleMemberships { get; set; }

    /// <summary>
    /// Gets or sets the set of invitation codes in the Get Together domain.
    /// </summary>
    public DbSet<InvitationCode>? InvitationCodes { get; set; }

    /// <summary>
    /// Gets or sets the set of events in the Get Together domain.
    /// </summary>
    public DbSet<Event>? Events { get; set; }

    /// <summary>
    /// Gets or sets the set of RSVP entries in the Get Together domain.
    /// </summary>
    public DbSet<RSVP>? Rsvps { get; set; }

    /// <summary>
    /// Gets or sets the set of reminder logs in the Get Together domain.
    /// </summary>
    public DbSet<ReminderLog>? ReminderLogs { get; set; }

    /// <summary>
    /// Configures the schema needed for the DadABase context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision for Rating property to match SQL column decimal(3,1)
        modelBuilder.Entity<Joke>()
            .Property(j => j.Rating)
            .HasPrecision(3, 1);

        // Configure composite key for JokeJokeCategory
        modelBuilder.Entity<JokeJokeCategory>()
            .HasKey(jjc => new { jjc.JokeId, jjc.JokeCategoryId });

        modelBuilder.Entity<User>()
            .HasIndex(u => u.ExternalId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailAddress)
            .IsUnique();

        modelBuilder.Entity<Circle>()
            .HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CirclesCreated)
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CircleMembership>()
            .HasIndex(cm => new { cm.CircleId, cm.UserId })
            .IsUnique();

        modelBuilder.Entity<CircleMembership>()
            .HasAlternateKey(cm => new { cm.CircleId, cm.UserId });

        modelBuilder.Entity<CircleMembership>()
            .HasOne(cm => cm.Circle)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CircleMembership>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.CircleMemberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvitationCode>()
            .HasIndex(ic => ic.Code)
            .IsUnique();

        modelBuilder.Entity<InvitationCode>()
            .HasIndex(ic => new { ic.CircleId, ic.CreatedUtc });

        modelBuilder.Entity<InvitationCode>()
            .HasOne(ic => ic.Circle)
            .WithMany(c => c.InvitationCodes)
            .HasForeignKey(ic => ic.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InvitationCode>()
            .HasOne(ic => ic.CreatedByUser)
            .WithMany(u => u.InvitationCodesCreated)
            .HasForeignKey(ic => ic.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvitationCode>()
            .HasOne(ic => ic.RedeemedByUser)
            .WithMany(u => u.InvitationCodesRedeemed)
            .HasForeignKey(ic => ic.RedeemedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.CircleId, e.StartsUtc });

        modelBuilder.Entity<Event>()
            .HasAlternateKey(e => new { e.EventId, e.CircleId });

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Circle)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.CreatedByUser)
            .WithMany(u => u.EventsCreated)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RSVP>()
            .HasIndex(r => new { r.EventId, r.UserId })
            .IsUnique();

        modelBuilder.Entity<RSVP>()
            .HasOne(r => r.User)
            .WithMany(u => u.Rsvps)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RSVP>()
            .HasOne(r => r.CircleMembership)
            .WithMany(cm => cm.Rsvps)
            .HasPrincipalKey(cm => new { cm.CircleId, cm.UserId })
            .HasForeignKey(r => new { r.CircleId, r.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RSVP>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Rsvps)
            .HasPrincipalKey(e => new { e.EventId, e.CircleId })
            .HasForeignKey(r => new { r.EventId, r.CircleId })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReminderLog>()
            .HasIndex(rl => new { rl.EventId, rl.UserId, rl.SentUtc });

        modelBuilder.Entity<ReminderLog>()
            .HasOne(rl => rl.Event)
            .WithMany(e => e.ReminderLogs)
            .HasForeignKey(rl => rl.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReminderLog>()
            .HasOne(rl => rl.User)
            .WithMany(u => u.ReminderLogs)
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
