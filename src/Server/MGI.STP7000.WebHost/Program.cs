using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MGI.STP7000.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                    //logging.AddNLog("nlog.config");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel().UseStartup<Startup>();
                })
                //.UseNLog();
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    //.WriteTo.Debug()
                    //.WriteTo.ColoredConsole(Serilog.Events.LogEventLevel.Verbose)
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {Message}{NewLine}{Exception}", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                    .WriteTo.RollingFile("Logs\\Domitory-{Date}.log", fileSizeLimitBytes: 16 * 1024 * 1024, buffered: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, flushToDiskInterval: TimeSpan.FromSeconds(10)));
    }
}

