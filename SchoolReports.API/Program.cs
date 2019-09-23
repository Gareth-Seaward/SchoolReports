using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolReports.API.Data;
using SchoolReports.API.Models;

namespace SchoolReports.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
              var services = scope.ServiceProvider;
              try
              {
                var context = services.GetRequiredService<DataContext>();
                var usermanager = services.GetRequiredService<UserManager<User>>(); 
                var roleManager  = services.GetRequiredService<RoleManager<Role>>();
                context.Database.Migrate();
                Seed.SeedUsers(usermanager, roleManager);
              }
              catch(Exception ex)
              {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An Error occured during migration");
              }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
