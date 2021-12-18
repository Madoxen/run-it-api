using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteShare> RouteShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friend>()
                .HasKey(x => new { x.RequesterId, x.ReceiverId });

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Requester)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.RequesterId);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Receiver)
                .WithMany(u => u.ReverseFriends)
                .HasForeignKey(f => f.ReceiverId);


            modelBuilder.Entity<RouteShare>()
            .HasKey(x => new { x.RouteId, x.SharedToId });
        }

    }
}