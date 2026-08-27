using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StandardBank.Models;

namespace StandardBank.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Home/Index
        public ActionResult Index()
        {
            // If user is logged in, redirect to dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // GET: Home/About
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        // GET: Home/Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // GET: Home/Features
        public ActionResult Features()
        {
            var features = new List<FeatureViewModel>
            {
                new FeatureViewModel
                {
                    Icon = "fa-shield-alt",
                    Title = "Secure Banking",
                    Description = "Your money is protected with advanced encryption and security measures."
                },
                new FeatureViewModel
                {
                    Icon = "fa-mobile-alt",
                    Title = "Mobile Banking",
                    Description = "Access your accounts anytime, anywhere with our mobile-friendly platform."
                },
                new FeatureViewModel
                {
                    Icon = "fa-chart-line",
                    Title = "Smart Insights",
                    Description = "Get AI-powered insights to help you manage your finances better."
                },
                new FeatureViewModel
                {
                    Icon = "fa-wallet",
                    Title = "Multiple Accounts",
                    Description = "Manage Cheque, Savings, and Student accounts all in one place."
                },
                new FeatureViewModel
                {
                    Icon = "fa-piggy-bank",
                    Title = "Savings Goals",
                    Description = "Set and track your savings goals with progress visualization."
                },
                new FeatureViewModel
                {
                    Icon = "fa-bullseye",
                    Title = "Budget Tracking",
                    Description = "Stay on top of your spending with category-based budgeting."
                }
            };

            return View(features);
        }

        // GET: Home/Pricing
        public ActionResult Pricing()
        {
            var plans = new List<PricingPlanViewModel>
            {
                new PricingPlanViewModel
                {
                    Name = "Basic",
                    Price = "Free",
                    Description = "Essential banking features for everyday use.",
                    Features = new List<string>
                    {
                        "1 Cheque Account",
                        "1 Savings Account",
                        "Send Money",
                        "Bill Payments",
                        "Transaction History"
                    },
                    IsPopular = false,
                    ButtonText = "Get Started",
                    ButtonClass = "btn-outline-primary"
                },
                new PricingPlanViewModel
                {
                    Name = "Plus",
                    Price = "$9.99",
                    Description = "Advanced features for power users.",
                    Features = new List<string>
                    {
                        "Unlimited Accounts",
                        "Budget Tracking",
                        "Savings Goals",
                        "AI Spending Insights",
                        "PDF Statements",
                        "Priority Support"
                    },
                    IsPopular = true,
                    ButtonText = "Subscribe Now",
                    ButtonClass = "btn-primary"
                },
                new PricingPlanViewModel
                {
                    Name = "Premium",
                    Price = "$19.99",
                    Description = "Complete banking solution.",
                    Features = new List<string>
                    {
                        "Everything in Plus",
                        "Customizable Dashboard",
                        "Investment Tracking",
                        "Tax Reports",
                        "Dedicated Account Manager",
                        "24/7 Premium Support"
                    },
                    IsPopular = false,
                    ButtonText = "Contact Sales",
                    ButtonClass = "btn-outline-primary"
                }
            };

            return View(plans);
        }

        // GET: Home/FAQ
        public ActionResult FAQ()
        {
            var faqs = new List<FAQViewModel>
            {
                new FAQViewModel
                {
                    Question = "How do I create an account?",
                    Answer = "Click on the 'Register' button in the top right corner. Fill in your details including your email, phone number, and ID number. Once registered, you'll get access to your dashboard."
                },
                new FAQViewModel
                {
                    Question = "Is my money safe?",
                    Answer = "Yes! We use bank-grade encryption and security protocols to protect your data and money. All transactions are monitored for suspicious activity."
                },
                new FAQViewModel
                {
                    Question = "How do I send money?",
                    Answer = "Go to 'Send Money' from your dashboard. Select your account, enter the recipient's account number, amount, and description. Confirm the transaction."
                },
                new FAQViewModel
                {
                    Question = "What types of accounts can I open?",
                    Answer = "You can open Cheque, Savings, and Student accounts. Each account type has different features and benefits to suit your needs."
                },
                new FAQViewModel
                {
                    Question = "How do I set up a savings goal?",
                    Answer = "From the 'Savings' section, click 'Create Goal'. Enter your goal name, target amount, and deadline. You can track your progress and add funds anytime."
                },
                new FAQViewModel
                {
                    Question = "Can I pay bills through the app?",
                    Answer = "Yes! You can pay Electricity, Water, DSTV, Internet, and other bills directly from your account. Go to 'Pay Bill' and follow the instructions."
                },
                new FAQViewModel
                {
                    Question = "How do I reset my password?",
                    Answer = "Click on 'Forgot Password' on the login page. Enter your email and we'll send you a link to reset your password securely."
                },
                new FAQViewModel
                {
                    Question = "What are the fees?",
                    Answer = "Basic accounts are free with no monthly fees. Plus and Premium plans offer additional features with affordable monthly subscriptions."
                }
            };

            return View(faqs);
        }

        // GET: Home/Support
        public ActionResult Support()
        {
            return View();
        }

        // POST: Home/Support
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Support(SupportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In production, send email to support team
            // For now, just show success message
            TempData["SuccessMessage"] = "Your message has been sent! Our support team will get back to you within 24 hours.";

            return RedirectToAction("Support");
        }

        // GET: Home/Terms
        public ActionResult Terms()
        {
            return View();
        }

        // GET: Home/Privacy
        public ActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Careers
        public ActionResult Careers()
        {
            return View();
        }

        // GET: Home/News
        public ActionResult News()
        {
            return View();
        }

        // GET: Home/Blog
        public ActionResult Blog()
        {
            return View();
        }

        // GET: Home/Security
        public ActionResult Security()
        {
            return View();
        }

        // GET: Home/Testimonials
        public ActionResult Testimonials()
        {
            var testimonials = new List<TestimonialViewModel>
            {
                new TestimonialViewModel
                {
                    Name = "John Doe",
                    Location = "Johannesburg",
                    Message = "Standard Bank has transformed how I manage my money. The dashboard is intuitive and the insights are incredibly helpful.",
                    Rating = 5
                },
                new TestimonialViewModel
                {
                    Name = "Jane Smith",
                    Location = "Cape Town",
                    Message = "The savings goals feature helped me save for my dream vacation. I love seeing my progress visualized.",
                    Rating = 5
                },
                new TestimonialViewModel
                {
                    Name = "Mike Johnson",
                    Location = "Durban",
                    Message = "Best online banking experience I've had. The budget tracking keeps me accountable for my spending.",
                    Rating = 4
                }
            };

            return View(testimonials);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Additional ViewModels for HomeController
    public class FeatureViewModel
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class PricingPlanViewModel
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public bool IsPopular { get; set; }
        public string ButtonText { get; set; }
        public string ButtonClass { get; set; }
    }

    public class FAQViewModel
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class SupportViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Full Name")]
        public string FullName { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Subject")]
        public string Subject { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Message")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Message { get; set; }

        public List<string> Subjects { get; set; } = new List<string>
        {
            "Account Issue",
            "Transaction Problem",
            "Technical Support",
            "Feature Request",
            "Feedback",
            "Other"
        };
    }

    public class TestimonialViewModel
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
    }
}