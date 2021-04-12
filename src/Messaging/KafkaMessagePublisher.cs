using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Messaging
{
    /// <summary>
    /// Publishes messages on the Kafka bus
    /// </summary>
    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<KafkaMessagePublisher> _logger;
        private readonly MessagePublisherOptions _options;
        private IProducer<Null, string> _producer;

        /// <summary>
        /// Initializes a new instance of <see cref="KafkaMessagePublisher"/>.
        /// </summary>
        /// <param name="options">Options for the message publisher.</param>
        public KafkaMessagePublisher(MessagePublisherOptions options, ILogger<KafkaMessagePublisher> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Publishes a message to a Kafka topic
        /// </summary>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="payload">Payload of the message</param>
        /// <returns>Returns the ID of the message</returns>
        public async Task PublishAsync(string topic, object payload)
        {
            try
            {
                EnsureProducer();

                var messageBody = JsonSerializer.Serialize(payload);
                var message = new Message<Null, string>
                {
                    Value = messageBody
                };

                await _producer.ProduceAsync(topic, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to {Topic}", topic);
                throw;
            }
        }

        private void EnsureProducer()
        {
            if (_producer != null)
            {
                return;
            }

            var config = new ProducerConfig
            {
                BootstrapServers = _options.Servers,
                ClientId = _options.ApplicationId
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }
    }
}
