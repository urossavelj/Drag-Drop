using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Db
{
    public class SettingsContext : DbContext
    {
        public DbSet<Settings> Settings => Set<Settings>();

        public string DbPath { get; }

        public SettingsContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "settings.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
