using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace StandardBank.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
    
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Disable cascade delete for BillPayment relationships
            modelBuilder.Entity<BillPayment>()
                .HasRequired(b => b.User)
                .WithMany(u => u.BillPayments)
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BillPayment>()
                .HasRequired(b => b.Account)
                .WithMany()
                .HasForeignKey(b => b.AccountId)
                .WillCascadeOnDelete(false);
        }
    } }