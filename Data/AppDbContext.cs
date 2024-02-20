using TaskManager.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions options): base(options) {}
        public DbSet<User> Users {get;set;}
        public DbSet<Tasks> Tasks{get;set;} 

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();

            modelBuilder.Entity<Tasks>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u=> u.CreatedTasks)
            .HasForeignKey(t => t.CreatedByUserID)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tasks>()
            .HasOne(t=> t.AssignedToUser)
            .WithMany(u=> u.AssignedTasks)
            .HasForeignKey(t=> t.AssignedToUserID)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}