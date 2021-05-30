using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using Timingz;

namespace WebApiSample
{
    public static class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureLogging(builder => builder.AddOpenTelemetry(options =>
                {
                    //options.IncludeScopes = true;
                    //options.ParseStateValues = true;
                    //options.IncludeFormattedMessage = true;
                    options.AddConsoleExporter();
                }));
    }
}
