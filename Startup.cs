using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using StandardBank;
using StandardBank.Models;
using System;

[assembly: OwinStartup(typeof(Startup))]

namespace StandardBank
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/Login")
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie("ExternalCookie");

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie("TwoFactorCookie", TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this box, your second step of verification will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie("TwoFactorRememberBrowser");

            // Uncomment the following lines to enable logging in with third party login providers
            // app.UseMicrosoftAccountAuthentication(
            //     clientId: "",
            //     clientSecret: "");

            // app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            // app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            // app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            // {
            //     ClientId = "",
            //     ClientSecret = ""
            // });

            app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
        }
    }
}