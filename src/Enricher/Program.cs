using Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Enricher
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var publisherOptions = context.Configuration
                        .GetSection("Publisher")
                        .Get<MessagePublisherOptions>();

                    var consumerOptions = context.Configuration
                        .GetSection("Consumer")
                        .Get<MessageConsumerOptions>();

                    var analyticsOptions = context.Configuration
                        .GetSection("Analytics")
                        .Get<TextAnalyticsOptions>();

                    services.AddMessagePublisher(publisherOptions);
                    services.AddMessageConsumer<RawNewsItem>(consumerOptions);

                    services.AddSingleton<IMessageHandler<RawNewsItem>, RawNewsItemProcessor>();
                    services.AddSingleton<ISentimentAnalysis>(serviceProvider => new SentimentAnalysis(analyticsOptions));
                })
                .Build();

            await host.RunAsync();
        }
    }
}
