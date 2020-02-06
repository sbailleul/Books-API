using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Books.Api.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Books.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetService<BooksContext>();
                    dbContext.Database.Migrate();
                }
                catch(Exception ex)
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during database migration");
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
