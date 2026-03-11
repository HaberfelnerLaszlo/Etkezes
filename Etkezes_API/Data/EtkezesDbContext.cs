using Microsoft.EntityFrameworkCore;
using Etkezes_Models;
namespace Etkezes_API.Data;

public class EtkezesDbContext : DbContext
{
    public EtkezesDbContext(DbContextOptions<EtkezesDbContext> options)
        : base(options)
    { }
    public DbSet<User> Users =>Set<User>();
    public DbSet<LoginUser> LoginUsers =>Set<LoginUser>();
    public DbSet<Etkezes> Etkezesek => Set<Etkezes>();
    public DbSet<SyncData> SyncDatas =>Set<SyncData>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.Id)
            .ValueGeneratedNever();
    }

}
