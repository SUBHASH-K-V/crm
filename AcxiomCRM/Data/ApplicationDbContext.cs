using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Customer>()
                .HasIndex(c => c.Email).IsUnique();
            builder.Entity<Customer>()
                .HasIndex(c => c.Phone).IsUnique();
            builder.Entity<Customer>()
                .HasIndex(c => c.CustomerCode).IsUnique();

            builder.Entity<Lead>()
                .HasIndex(l => l.LeadCode).IsUnique();

            builder.Entity<Customer>()
                .HasMany(c => c.Leads)
                .WithOne(l => l.Customer)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Customer>()
                .HasMany(c => c.Opportunities)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Customer>()
                .HasMany(c => c.FollowUps)
                .WithOne(f => f.Customer)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Customer>()
                .HasMany(c => c.Activities)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lead>()
                .HasMany(l => l.FollowUps)
                .WithOne(f => f.Lead)
                .HasForeignKey(f => f.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lead>()
                .HasMany(l => l.Activities)
                .WithOne(a => a.Lead)
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Opportunity>()
                .HasMany(o => o.FollowUps)
                .WithOne(f => f.Opportunity)
                .HasForeignKey(f => f.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Opportunity>()
                .HasMany(o => o.Activities)
                .WithOne(a => a.Opportunity)
                .HasForeignKey(a => a.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
