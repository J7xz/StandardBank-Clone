using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StandardBank.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace StandardBank.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            // Create Admin role if it doesn't exist
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            // Create Admin user if it doesn't exist
            if (!context.Users.Any(u => u.Email == "admin@standardbank.com"))
            {
                var userStore = new UserStore<User>(context);
                var userManager = new UserManager<User>(userStore);

                var adminUser = new User
                {
                    UserName = "admin@standardbank.com",
                    Email = "admin@standardbank.com",
                    FullName = "System Administrator",
                    PhoneNumber = "0712345678",
                    IDNumber = "8001015001088",
                    DateRegistered = DateTime.Now,
                    IsActive = true
                };

                var result = userManager.Create(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }
        }
    }
}