using Microsoft.EntityFrameworkCore;
using Etkezes_Models;
namespace Etkezes_API.Data;

public class EtkezesDbContext : DbContext
{
    public EtkezesDbContext(DbContextOptions<EtkezesDbContext> options)
        : base(options)
    { }
    public DbSet<User> Users { get; set; }
    public DbSet<LoginUser> LoginUsers { get; set; }
    public DbSet<Etkezes> Etkezesek { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.Id)
            .ValueGeneratedNever();
    }

}
