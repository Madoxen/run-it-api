using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class AppContext : DbContext
    {
        public AppContext (DbContextOptions<AppContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

    }
}