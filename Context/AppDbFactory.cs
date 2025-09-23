using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace LoginAPI.Context
{
    public class AppDbFactory : IDesignTimeDbContextFactory<AppDb>
    {
        public AppDb CreateDbContext(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = cfg.GetConnectionString("Default") ?? "Data Source=app.db";

            var opts = new DbContextOptionsBuilder<AppDb>()
                .UseSqlite(cs)
                .Options;

            return new AppDb(opts);
        }
    }
}
