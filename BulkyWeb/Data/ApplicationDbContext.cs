using BulkyWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BulkyWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" , DisplayOrder = 1},
                new Category { Id = 2, Name = "Clothing", DisplayOrder = 3 },
                new Category { Id = 3, Name = "Home Appliances", DisplayOrder = 2}
                );
        }
    }
}
