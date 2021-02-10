using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Generator
{
    public class MessageGeneratorService : IHostedService
    {
        private Thread _senderThread;
        private CancellationTokenSource _cancellationTokenSource;
        private IProducer<Null, string> _producer;
        private Random _random;

        private ILogger<MessageGeneratorService> _logger;

        private string[] _titles =
        {
            "WHO-team: oorsprong virus niet per se op vismarkt Wuhan, lab als bron onwaarschijnlijk",
            "Deel lading binnenvaartschepen nu over de weg wegens ijs en hoogwater"
        };

        private string[] _bodies =
        {
            "Het team van de Wereldgezondheidsorganisatie (WHO), met ook de Nederlandse viroloog Marion Koopmans, heeft bijna een maand onderzoek gedaan.",
            "Hoogwater, sneeuw en ijs zitten de binnenvaart de afgelopen dagen flink dwars. Daarom verplaatsen binnenvaartschippers een deel van het transport naar de weg."
        };

        public MessageGeneratorService(ILogger<MessageGeneratorService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _random = new Random();
            _cancellationTokenSource = new CancellationTokenSource();
            _senderThread = new Thread(SendLoop);
            _senderThread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void SendLoop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var index = _random.Next(_titles.Length);
                var messageId = PublishMessage(_titles[index], _bodies[index]).Result;

                _logger.LogInformation("Published message {MessageId}", messageId);

                Thread.Sleep(1000);
            }
        }

        private async Task<Guid> PublishMessage(string title, string body)
        {
            EnsureProducer();

            var messageId = Guid.NewGuid();
            var messageBody = JsonSerializer.Serialize(new NewsItem(messageId, title, body));
            var message = new Message<Null, string>
            {
                Value = messageBody
            };

            await _producer.ProduceAsync("newsitems", message);

            return messageId;
        }

        private void EnsureProducer()
        {
            if (_producer != null)
            {
                return;
            }

            var config = new ProducerConfig
            {
                BootstrapServers = "broker:9092",
                ClientId = "generator"
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }
    }
}
