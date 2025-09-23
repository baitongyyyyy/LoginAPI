using LoginAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace LoginAPI.Context
{
    public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
           builder.Entity<User>().HasIndex(t => t.UserName).IsUnique();
        }
    }
}
