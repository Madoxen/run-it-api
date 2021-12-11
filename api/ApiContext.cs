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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friend>()
                .HasKey(x => new { x.RequesterId, x.ReceiverId });

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId);
        }

    }
}