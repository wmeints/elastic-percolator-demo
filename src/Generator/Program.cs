using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Threading.Tasks;

namespace Generator
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MessageGeneratorService>();
                })
                .ConfigureLogging((ILoggingBuilder logging) =>
                {
                    logging.AddConsole();
                })
                .Build();


            await host.RunAsync();
        }
    }
}
