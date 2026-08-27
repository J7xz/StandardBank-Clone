using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StandardBank.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string IDNumber { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }

        // Navigation properties
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Beneficiary> Beneficiaries { get; set; }
        public virtual ICollection<BillPayment> BillPayments { get; set; }
        public virtual ICollection<SavingsGoal> SavingsGoals { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        // ADD THIS METHOD
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}