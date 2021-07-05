using System;
using System.IO;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            try
            {
                logger.LogDebug("Getting DataContext from Services");
                var context = services.GetRequiredService<DataContext>();

                logger.LogDebug("About to run migrate");
                await context.Database.MigrateAsync();

                logger.LogDebug("About to seed users");
                await Seed.SeedUsers(context);

                logger.LogDebug("Seed Users successfully");
            }
            catch (Exception ex)
            {
                
                logger.LogError("Error during migration, lols", ex);
            }
            await host.RunAsync().ConfigureAwait(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    // find the shared folder in the parent folder
                    var sharedFolder = Path.Combine(env.ContentRootPath, "appsettings.Local.json");
                    config.AddJsonFile(sharedFolder, optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
