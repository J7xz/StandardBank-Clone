using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StandardBank.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace StandardBank.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup/CreateAdmin
        public ActionResult CreateAdmin()
        {
            using (var context = new ApplicationDbContext())
            {
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
                        ViewBag.Message = "✅ Admin created successfully!";
                    }
                    else
                    {
                        ViewBag.Message = "❌ Failed to create admin: " + string.Join(", ", result.Errors);
                    }
                }
                else
                {
                    ViewBag.Message = "ℹ️ Admin already exists.";
                }
            }

            return View();
        }
    }
}