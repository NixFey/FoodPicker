using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using FoodPicker.Web.Enums;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodPicker.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly("FoodPicker.Migrations")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "FoodPicker";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.LoginPath = "/Auth/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = (ctx) =>
                {
                    // Allow automatic login
                    var redirectPath = ctx.RedirectUri;
                    if (redirectPath.Contains("?ReturnUrl"))
                    {
                        redirectPath += "&";
                    }
                    else
                    {
                        redirectPath += "?";
                    }

                    redirectPath += "autologin=true";
                    ctx.Response.Redirect(redirectPath);
                    return Task.CompletedTask;
                };
            });
            services.AddControllersWithViews();
            
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(AuthorizationPolicies.AccessInternalAdminAreas, pol =>
                {
                    pol.RequireRole("Admin");
                    pol.RequireClaim("PasswordLogin", "true");
                });

                // NOTE: Enabling the API allows clients to perform actions without authenticating first. Ensure your
                // installation is secure before setting the "EnableApi" config setting to true
                if (!bool.TryParse(Configuration["EnableApi"], out var apiIsEnabled))
                {
                    apiIsEnabled = false;
                }
                
                opt.AddPolicy(AuthorizationPolicies.AllowApi, pol =>
                {
                    pol.RequireAssertion((_) => apiIsEnabled);
                });
            });
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                if (!Configuration.GetSection("KnownProxies").Exists()) return;
                
                foreach (var proxy in Configuration.GetSection("KnownProxies").Get<string[]>())
                {
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                }
            });

            var redirectToHttps = true;
            if (Configuration["RedirectToHttps"] != null && !bool.TryParse(Configuration["RedirectToHttps"], out redirectToHttps))
            {
                throw new ApplicationException(
                    "The configuration value for `RedirectToHttps` is invalid. It should be `true` or `false`.");
            }
            if (redirectToHttps)
            {
                // We're not running as HTTPS in the container, but we can trust the load balancer to be listening on 443
                services.AddHttpsRedirection(opt =>
                {
                    opt.HttpsPort = 443;
                });
            }

            switch (Configuration["MealService"])
            {
                case "HelloFresh":
                    services.AddScoped<MealService, HelloFreshMealService>();
                    services.AddHostedService<HelloFreshRefreshService>();
                    break;
                case "HomeChef":
                    services.AddScoped<MealService, HomeChefMealService>();
                    break;
                case null:
                    throw new ApplicationException("No `MealService` was provided in the configuration file");
                default:
                    throw new ApplicationException($"Unknown meal service {Configuration["MealService"]}");
            }

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            var repoTypes = typeof(Repository).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Repository)));
            
            foreach(var repoType in repoTypes )
            {
                services.AddScoped(repoType);
            }
            
            var serviceTypes = typeof(IService).Assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(IService)) && x != typeof(IService));
            foreach(var serviceType in serviceTypes )
            {
                services.AddScoped(serviceType);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            dbContext.Database.Migrate();

            app.UseAuthentication();
            app.UseAuthorization();

            RoleDataInitializer.SeedData(roleManager).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}