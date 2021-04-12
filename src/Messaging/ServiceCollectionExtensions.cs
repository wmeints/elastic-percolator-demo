using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Messaging
{
    /// <summary>
    /// Extension methods for the service collection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a message consumer to the application to handle incoming message of a specific type.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to consume.</typeparam>
        /// <param name="serviceCollection">Service collection to add the consumer to.</param>
        /// <param name="options">Options for the consumer.</param>
        public static IServiceCollection AddMessageConsumer<TMessage>(this IServiceCollection serviceCollection, MessageConsumerOptions options)
        {
            serviceCollection.AddHostedService(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<KafkaConsumerService<TMessage>>>();
                return new KafkaConsumerService<TMessage>(options, logger, serviceProvider);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddMessagePublisher(this IServiceCollection serviceCollection, MessagePublisherOptions options)
        {
            serviceCollection.AddSingleton<IMessagePublisher>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<KafkaMessagePublisher>>();
                return new KafkaMessagePublisher(options, logger);
            });

            return serviceCollection;
        }
    }
}
