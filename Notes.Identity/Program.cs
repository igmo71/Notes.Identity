using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Notes.Identity.Data;
using Notes.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Notes.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;

            var connctionString = builder.Configuration.GetValue<string>("DbConnection");
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlite(connctionString);
            });

            services.AddIdentity<AppUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Notes.Identity.Cookie";
                config.LoginPath = "/Auth/Login";
                config.LogoutPath = "/Auth/Logout";
            });

            services.AddIdentityServer()
                .AddAspNetIdentity<AppUser>()
                .AddInMemoryApiResources(new List<ApiResource>(Configuration.ApiResources))
                .AddInMemoryIdentityResources(new List<IdentityResource>(Configuration.IdentityResources))
                .AddInMemoryApiScopes(new List<ApiScope>(Configuration.ApiScopes))
                .AddInMemoryClients(new List<Client>(Configuration.Clients))
                .AddDeveloperSigningCredential();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var servicePrivider = scope.ServiceProvider;
                try
                {
                    var context = servicePrivider.GetRequiredService<AuthDbContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception exception)
                {
                    var logger = servicePrivider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "Error occured while app initialization");
                    //throw;
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseIdentityServer();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}