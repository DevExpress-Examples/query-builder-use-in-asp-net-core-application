using Microsoft.EntityFrameworkCore;

namespace AspNetCoreQueryBuilderApp.Data {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ReportEntity> Reports { get; set; }
        public DbSet<DataSourceEntity> DataSources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ReportEntity>().ToTable("Reports");
            modelBuilder.Entity<DataSourceEntity>().ToTable("DataSources");
        }
    }
}
