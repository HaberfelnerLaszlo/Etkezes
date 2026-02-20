using Etkezes_Models;

using Microsoft.EntityFrameworkCore;

using System.Reflection.Metadata;

namespace Etkezes_Ellenor.Data
{
    public class EtkezesDBcontext : DbContext
    {     
        public string DbPath { get; private set; }

        public EtkezesDBcontext(DbContextOptions<EtkezesDBcontext> options)
            : base(options)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "etkezes_local.db");
        }

        public DbSet<Etkezok> Etkezesek { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoginUser> LoginUsers { get; set; }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(b => b.Id)
                .ValueGeneratedNever();
        }
    }
}
