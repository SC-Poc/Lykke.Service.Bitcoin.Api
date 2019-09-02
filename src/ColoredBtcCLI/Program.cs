using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ColoredBtcCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceProvider serviceProvider = null;
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("settings.json")
                    .Build();
                
                var services = new ServiceCollection();

                services.AddLogging(logging => { logging.AddConsole(); });
                services.AddOptions();
                services.AddHttpClient();
                services.AddSingleton(configuration.Get<Settings>());
                services.AddSingleton<ColoredBtcClITool>();
                
                serviceProvider = services.BuildServiceProvider();

                serviceProvider.GetRequiredService<ColoredBtcClITool>().Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"args: {string.Join(", ", args)}. {ex}");
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }
    }
}
