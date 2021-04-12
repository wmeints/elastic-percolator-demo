using Confluent.Kafka;
using Messaging;
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
        private IMessagePublisher _messagePublisher;
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

        public MessageGeneratorService(IMessagePublisher messagePublisher, ILogger<MessageGeneratorService> logger)
        {
            _messagePublisher = messagePublisher;
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
                var message = new NewsItem(Guid.NewGuid(), _titles[index], _bodies[index]);
                var publishTask = _messagePublisher.PublishAsync("raw-newsitems", message);

                publishTask.ConfigureAwait(false);
                publishTask.Wait();

                _logger.LogInformation("Published message {MessageId}", message.Id);

                Thread.Sleep(1000);
            }
        }

    }
}
