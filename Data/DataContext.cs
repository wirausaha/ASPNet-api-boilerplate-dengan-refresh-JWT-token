using Microsoft.EntityFrameworkCore;
using AspApi.Models;


namespace AspApi.Data
{
    public class DataContext : DbContext
    {
       public DataContext(DbContextOptions<DataContext> options) : base(options) { }

       public DbSet<AspApi.Models.User> Users { get; set; }

       public DbSet<AspApi.Models.SysToken> SysTokens { get; set; }

	
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
//      modelBuilder.Entity<AspApi.Models.UserCompany>()
//          .HasKey(uc => new { uc.UserName, uc.CompanyCode });

      // base.OnModelCreating(modelBuilder);

    }


    }
}
