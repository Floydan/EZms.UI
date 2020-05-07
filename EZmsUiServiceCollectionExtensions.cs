using System;
using System.Collections.Generic;
using System.Text;
using EZms.Core;
using EZms.UI.Infrastructure.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.Extensions.DependencyInjection;

namespace EZms.UI
{
    public static class EZmsUIServiceCollectionExtensions
    {
        public static IServiceCollection AddEZmsUI(this IServiceCollection services)
        {
            services.ConfigureOptions(typeof(EZmsUIConfigureOptions));

            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Default User settings.
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/EZmsIdentity/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/EZmsIdentity/Account/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddUserManager<UserManager<IdentityUser>>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<EZmsContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<UserManager<IdentityUser>>();

            services.AddSingleton<EditorReflectionService>();

            return services;
        }
    }
}
