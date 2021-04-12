using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Messaging
{
    public class KafkaConsumerService<TMessage> : IHostedService
    {
        private Thread _receiverThread;
        private IConsumer<Null, string> _consumer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<KafkaConsumerService<TMessage>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageConsumerOptions _options;

        public KafkaConsumerService(MessageConsumerOptions options, ILogger<KafkaConsumerService<TMessage>> logger, IServiceProvider serviceProvider)
        {
            _options = options;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting kafka consumer for {Topic}", _options.Topic);

            var config = new ConsumerConfig
            {
                BootstrapServers = _options.Servers,
                AutoCommitIntervalMs = 0,
                Acks = Acks.Leader,
                GroupId = _options.ApplicationId
            };

            _consumer = new ConsumerBuilder<Null, string>(config).Build();
            _receiverThread = new Thread(ReceiveLoop);

            _receiverThread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping kafka consumer for {Topic}", _options.Topic);

            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void ReceiveLoop()
        {
            _consumer.Subscribe(_options.Topic);

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {      
                    var result = _consumer.Consume(200);

                    if(result == null)
                    {
                        continue;
                    }
                    
                    var messageBody = JsonSerializer.Deserialize<TMessage>(result.Message.Value);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var eventHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
                        var handlerTask = eventHandler.HandleAsync(messageBody);

                        handlerTask.ConfigureAwait(false);
                        handlerTask.Wait();
                    }

                    _consumer.Commit(new[] { result.TopicPartitionOffset });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message");
                }
            }
        }
    }
}