using Etkezes_Models;

using Microsoft.EntityFrameworkCore;

using System.Reflection.Metadata;

namespace Etkezes_Ellenor.Data
{
    public class EtkezesDBcontext(DbContextOptions<EtkezesDBcontext> options) : DbContext(options)
    {
        //public string DbPath { get; private set; }


        public DbSet<Etkezok> Etkezesek { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoginUser> LoginUsers { get; set; }
        public DbSet<SyncData> SyncDatas =>Set<SyncData>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source=etkezes_local.db");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(b => b.Id)
                .ValueGeneratedNever();
        }
    }
}
