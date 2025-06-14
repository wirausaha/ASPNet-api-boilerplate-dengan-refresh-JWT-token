using Microsoft.EntityFrameworkCore;
using AspApi.Models;


namespace AspApi.Data
{
    public class DataContext : DbContext
    {
       public DataContext(DbContextOptions<DataContext> options) : base(options) { }

       public DbSet<AspApi.Models.User> Users { get; set; }

       public DbSet<AspApi.Models.SysToken> SysTokens { get; set; }

	
    // this is for PostgreSQL database connection, shoul be modified if using other database engines
    // Bagian ini harus disesuaikan dengan database engine yang digunakan
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<SysToken>()
        .Property(e => e.ExpireDate)
        .HasColumnType("timestamp without time zone") 
        .HasConversion(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));
   

        base.OnModelCreating(modelBuilder);

    }


    }
}
