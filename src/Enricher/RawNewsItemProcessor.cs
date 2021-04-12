using Messaging;
using System.Threading.Tasks;

namespace Enricher
{
    public class RawNewsItemProcessor : IMessageHandler<RawNewsItem>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ISentimentAnalysis _sentimentAnalysis;

        public RawNewsItemProcessor(IMessagePublisher messagePublisher, ISentimentAnalysis sentimentAnalysis)
        {
            _messagePublisher = messagePublisher;
            _sentimentAnalysis = sentimentAnalysis;
        }

        public async Task HandleAsync(RawNewsItem messageBody)
        {
            var analysisOutcome = await _sentimentAnalysis.PredictAsync(messageBody.Body);
            var enrichedNewsItem = new EnrichedNewsItem(messageBody.Id, messageBody.Title, messageBody.Body, analysisOutcome);

            await _messagePublisher.PublishAsync("enriched-newsitems", enrichedNewsItem);
        }
    }
}
