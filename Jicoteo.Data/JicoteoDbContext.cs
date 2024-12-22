using LSCC.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace LSCC.Data
{
    public class JicoteoDbContext : DbContext
    {
        private const string dbName = "fonts.db";

        public JicoteoDbContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, dbName);
        }

        public DbSet<ClientModel>? Clients { get; set; }

        public string DbPath
        {
            get;
        }

        public DbSet<MessageTriggerDataModel>? MessageTriggers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
                    => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<YourEntity>()
            //.Property(e => e.Strings)
            //.HasConversion(
            //    v => string.Join(',', v),
            //    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}