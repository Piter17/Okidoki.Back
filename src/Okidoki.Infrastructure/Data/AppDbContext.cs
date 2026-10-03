using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Okidoki.Domain.Entities;

namespace Okidoki.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Identity, ApplicationRole, long>(options)
{
    public DbSet<User> Accounts { get; set; } = default!;
    public DbSet<FriendRequest> FriendRequests { get; set; } = default!;
    public DbSet<FriendRelation> FriendRelations { get; set; } = default!;
    public DbSet<Guild> Guilds { get; set; } = default!;
    public DbSet<GuildProfile> GuildProfiles { get; set; } = default!;
    public DbSet<GuildChannel> GuildChannels { get; set; } = default!;
    public DbSet<GuildChannelGroup> GuildChannelGroups { get; set; } = default!;
    public DbSet<GuildRole> GuildRoles { get; set; } = default!;
    public DbSet<GuildMembership> GuildUsers { get; set; } = default!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
    public DbSet<GuildInvitation> GuildInvitations { get; set; } = default!;
    public DbSet<GuildEventLog> GuildEventLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Identity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique();

            b.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<User>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasAlternateKey(e => e.Username);
        });

        modelBuilder.Entity<Guild>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.HasOne(x => x.Owner)
                .WithMany(u => u.OwnedGuilds)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GuildRole>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => new { x.Id, x.GuildId });
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.HasOne(x => x.Guild)
                .WithMany(g => g.Roles)
                .HasForeignKey(x => x.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuildMembership>(entity =>
        {
            entity.ToTable("GuildMembership");
            entity.HasKey(x => new { x.UserId, x.GuildId });
        });

        modelBuilder.Entity<GuildMembership>()
            .HasOne(x => x.User)
            .WithMany(u => u.GuildMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildMembership>()
            .HasOne(x => x.Guild)
            .WithMany(g => g.GuildUsers)
            .HasForeignKey(x => x.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildMembership>()
            .HasMany(x => x.Roles)
            .WithMany(r => r.UsersMembership)
            .UsingEntity<Dictionary<string, object>>("GuildMembershipRole",
                j => j.HasOne<GuildRole>().WithMany()
                    .HasForeignKey("RoleId", "GuildId")
                    .HasPrincipalKey(x => new { x.Id, x.GuildId })
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<GuildMembership>().WithMany().HasForeignKey("UserId", "GuildId").OnDelete(DeleteBehavior.NoAction));

        modelBuilder.Entity<GuildEventLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.HasOne(x => x.Guild)
                .WithMany(x => x.EventLogs)
                .HasForeignKey(x => x.GuildId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FriendRelation>()
            .HasKey(x => new { x.UserAId, x.UserBId });

        modelBuilder.Entity<FriendRelation>()
            .HasOne(x => x.UserA)
            .WithMany(u => u.FriendsAsUserA)
            .HasForeignKey(x => x.UserAId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FriendRelation>()
            .HasOne(x => x.UserB)
            .WithMany(u => u.FriendsAsUserB)
            .HasForeignKey(x => x.UserBId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FriendRequest>()
            .HasKey(x => new { x.SenderId, x.TargetId });

        modelBuilder.Entity<FriendRequest>()
            .HasOne(x => x.Sender)
            .WithMany(u => u.FriendRequests)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FriendRequest>()
            .HasOne(x => x.Target)
            .WithMany(u => u.FriendInvitations)
            .HasForeignKey(x => x.TargetId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<GuildProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<GuildChannel>(entity =>
        {
            entity.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<GuildInvitation>(entity =>
        {
            entity.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}
