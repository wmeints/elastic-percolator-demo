using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Messaging;
using Microsoft.Extensions.Configuration;

namespace Generator
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var publisherOptions = context.Configuration
                        .GetSection("Messaging")
                        .Get<MessagePublisherOptions>();

                    services.AddMessagePublisher(publisherOptions);
                    services.AddHostedService<MessageGeneratorService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
