using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Dejarix.App.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dejarix.App
{
    public class Startup
    {
        public static DateTime UtcStart { get; } = DateTime.UtcNow;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<Mailgun>();

            var connectionString = Configuration.GetConnectionString("PrimaryDatabase");
            var connectionFactory = new ConnectionFactory(connectionString);
            services.AddSingleton(connectionFactory);
            services.AddDbContext<DejarixDbContext>(connectionFactory.BuildOptions);

            services
                .AddIdentity<DejarixUser, IdentityRole<Guid>>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
                    options.Password.RequiredLength = 12;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<DejarixDbContext>()
                .AddDefaultTokenProviders();
            
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            
            services
                .AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            var factory = serviceProvider.GetService<ConnectionFactory>();
            using (var context = factory.CreateContext())
            {
                if (env.IsDevelopment() && Configuration.GetValue<bool>("DeleteDatabase"))
                    context.Database.EnsureDeleted();
                
                if (context.Database.EnsureCreated())
                {
                    // Seed important data!
                    var seedFile = Configuration["SeedFile"];
                    if (!string.IsNullOrEmpty(seedFile))
                        context.SeedDataAsync(seedFile).GetAwaiter().GetResult();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseExceptionLogger();
            app.UseStatusCodePagesWithReExecute("/status-code/{0}");
            app.UseStaticFiles();

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TradeHub>("/trade-hub");
            });
        }
    }
}
