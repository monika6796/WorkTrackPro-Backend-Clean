using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkTrackPro.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql(
    "Host=reseau.proxy.rlwy.net;Port=37249;Database=railway;Username=postgres;Password=fyPgyYvYpItIVOrjIEpXQXUQOQqAVyGm;SSL Mode=Require;Trust Server Certificate=true");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}