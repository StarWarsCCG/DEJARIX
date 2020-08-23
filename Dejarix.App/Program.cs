using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dejarix.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options => options.ListenLocalhost(5000));
                    webBuilder.ConfigureLogging(builder => 
                    {
                        builder.ClearProviders();
                        builder.AddConsole(options =>
                        {
                            options.DisableColors = false;
                            options.IncludeScopes = true;
                        });
                    }).UseStartup<Startup>();
                });
        }
    }
}
