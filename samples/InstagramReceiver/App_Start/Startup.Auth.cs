using System;
using System.Threading.Tasks;
using InstagramReceiver.Models;
using InstaSharp;
using InstaSharp.Models;
using InstaSharp.Models.Responses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Instagram;
using Owin.Security.Providers.Instagram.Provider;

namespace InstagramReceiver
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Get the config used by InstaSharp
            InstagramConfig config = Dependencies.InstagramConfig;

            // Wire up Instagram authentication so that we can access the media published by a user.
            var options = new InstagramAuthenticationOptions()
            {
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                Provider = new InstagramAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        // Retrieve the OAuth access token to store for subsequent API calls
                        OAuthResponse response = new OAuthResponse
                        {
                            User = new UserInfo
                            {
                                Id = long.Parse(context.Id),
                                FullName = context.Name,
                                ProfilePicture = context.ProfilePicture,
                                Username = context.UserName,
                            },
                            AccessToken = context.AccessToken,
                        };

                        string userId = context.Id;
                        string accessToken = context.AccessToken;

                        // Store the token in memory so that we can use it for accessing media. In a real scenario
                        // this would be stored somewhere else.
                        Dependencies.Tokens[userId] = response;
                        return Task.FromResult(true);
                    }
                }
            };

            app.UseInstagramInAuthentication(options);
        }
    }
}