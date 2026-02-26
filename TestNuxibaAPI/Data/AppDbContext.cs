using Microsoft.EntityFrameworkCore;
using TestNuxibaAPI.Models;

namespace TestNuxibaAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Area> Areas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Login> Logins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nombres de las tablas para que coincidan con el Excel y el examen
            modelBuilder.Entity<Area>().ToTable("ccRIACat_Areas");
            modelBuilder.Entity<User>().ToTable("ccUsers");
            modelBuilder.Entity<Login>().ToTable("ccloglogin");

            modelBuilder.Entity<Area>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().Property(u => u.User_id).ValueGeneratedNever();
            modelBuilder.Entity<Login>().Property(l => l.Id).ValueGeneratedOnAdd();
        }
    }
}