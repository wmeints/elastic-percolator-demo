using Confluent.Kafka;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Threading.Tasks;

namespace Generator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand(description: "Generates a news item");
            
            rootCommand.AddOption(new Option("--title", description: "The title of the news item")
            {
                Argument = new Argument<string>(),
                IsRequired = true
            });

            rootCommand.AddOption(new Option("--body", description: "The body of the news item")
            {
                Argument = new Argument<string>(),
                IsRequired = true
            });

            rootCommand.Handler = CommandHandler.Create<string, string>(PublishMessage);

            await rootCommand.InvokeAsync(args);
        }

        private static async Task PublishMessage(string title, string body)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                ClientId = "generator"
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                var messageBody = JsonSerializer.Serialize(new { title, body });
                var message = new Message<Null, string>
                {
                    Value = messageBody
                };

                await producer.ProduceAsync("newsitems", message);
            }
        }
    }
}
