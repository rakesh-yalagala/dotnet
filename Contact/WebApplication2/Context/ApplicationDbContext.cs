using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Context
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }
        public DbSet<Customers> NewCustomers { get; set; }
        public DbSet<AlterNewContact>NewNumber{set; get; }
        public DbSet<NewContacts>Contact{set; get; }
        
      
        protected override void OnModelCreating  (ModelBuilder modelBuilder)
        
        {

            modelBuilder.Entity<Customers>().ToTable("newcustomers");
            modelBuilder.Entity<AlterNewContact>().ToTable("newnumber");
            modelBuilder.Entity<NewContacts>().ToTable("contact");
       

    
        }
    }
}

