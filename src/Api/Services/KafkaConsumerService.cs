using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services
{
    public class KafkaConsumerService : IHostedService
    {
        private Thread _receiverThread;
        private IConsumer<Null, string> _consumer;
        private CancellationTokenSource _cancellationTokenSource;
        private ILogger<KafkaConsumerService> _logger;
        private IServiceProvider _serviceProvider;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting kafka consumer");

            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                AutoCommitIntervalMs = 500,
                Acks = Acks.Leader,
                GroupId = "api"
            };

            _consumer = new ConsumerBuilder<Null, string>(config).Build();
            _receiverThread = new Thread(ReceiveLoop);

            _receiverThread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping kafka consumer");

            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void ReceiveLoop()
        {
            _consumer.Subscribe("newsitems");

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {      
                    var result = _consumer.Consume(200);

                    if(result == null)
                    {
                        continue;
                    }
                    
                    _logger.LogInformation("Processing incoming message");

                    var messageBody = JsonSerializer.Deserialize<NewsItem>(result.Message.Value);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var eventHandler = scope.ServiceProvider.GetRequiredService<INewsItemEventHandler>();
                        var handlerTask = eventHandler.HandleAsync(messageBody);

                        handlerTask.ConfigureAwait(false);
                        handlerTask.Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message");
                }
            }
        }
    }
}