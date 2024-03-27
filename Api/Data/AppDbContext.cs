using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoomModel> Rooms { get; set; }
        public DbSet<ConnectedUserModel> ConnectedUsers { get; set; }
        public DbSet<JoinedRoomModel> JoinedRooms { get; set; }
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<UserPeerModel> UserPeers { get; set; }
        public DbSet<CallRoomModel> CallRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RoomModel>()
        .HasMany(r => r.Users)
        .WithMany(u => u.Rooms)
        .UsingEntity<Dictionary<string, object>>(
            "RoomUser",
            j => j
                .HasOne<UserModel>()
                .WithMany()
                .HasForeignKey("UserId"),
            j => j
                .HasOne<RoomModel>()
                .WithMany()
                .HasForeignKey("RoomId"),
            j =>
            {
                j.HasKey("RoomId", "UserId");
                j.ToTable("RoomUsers");
            }
        );

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                {
                    entity.Property("CreatedAt").CurrentValue = now;
                }

                entity.Property("UpdatedAt").CurrentValue = now;
            }
        }
    }
}
