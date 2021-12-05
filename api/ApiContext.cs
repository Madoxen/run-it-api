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
        public DbSet<Run> Runs { get; set; }
        public DbSet<Route> Routes { get; set; }
    }
}