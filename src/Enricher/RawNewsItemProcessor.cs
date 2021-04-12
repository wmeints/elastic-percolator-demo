using Messaging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Threading.Tasks;

namespace Enricher
{
    public class RawNewsItemProcessor : IMessageHandler<RawNewsItem>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ISentimentAnalysis _sentimentAnalysis;
        private readonly IAsyncPolicy<SentimentScoringOutcome> _policy;
        private readonly ILogger<RawNewsItemProcessor> _logger;

        public RawNewsItemProcessor(IMessagePublisher messagePublisher, ISentimentAnalysis sentimentAnalysis, ILogger<RawNewsItemProcessor> logger)
        {
            _messagePublisher = messagePublisher;
            _sentimentAnalysis = sentimentAnalysis;
            _logger = logger;

            _policy = CreateRetryPolicy();
        }

        public async Task HandleAsync(RawNewsItem messageBody)
        {
            var operationResult = await _policy.ExecuteAndCaptureAsync(async () => await _sentimentAnalysis.PredictAsync(messageBody.Body));

            if (operationResult.Outcome == OutcomeType.Successful)
            {
                _logger.LogInformation("Enriched item {ItemId} with sentiment score.", messageBody.Id);

                var enrichedNewsItem = new EnrichedNewsItem(messageBody.Id, messageBody.Title, messageBody.Body, operationResult.Result);
                await _messagePublisher.PublishAsync("enriched-newsitems", enrichedNewsItem);
            }
            else
            {
                _logger.LogError(operationResult.FinalException, "Couldn't enrich news item {ItemId}. Moving item to the invalid-letter queue.", messageBody.Id);

                await _messagePublisher.PublishAsync("invalid-newsitems", new EnrichmentFailure
                {
                    ErrorMessage = operationResult.FinalException.Message,
                    OriginalItem = messageBody
                });
            }
        }

        private IAsyncPolicy<SentimentScoringOutcome> CreateRetryPolicy()
        {
            return Policy<SentimentScoringOutcome>
                .Handle<Exception>()
                .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(100), (outcome, timeout) => 
                {
                    _logger.LogWarning("The operation failed. Retrying in {Timeout} ms", timeout.TotalMilliseconds);
                });
        }

        private IAsyncPolicy<SentimentScoringOutcome> CreateRetryWithTimeoutPolicy()
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2));

            var fallbackForTimeoutPolicy = Policy<SentimentScoringOutcome>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(token => Task.FromResult(new SentimentScoringOutcome("Neutral", 0.0, 0.0, 0.0)), fallback => 
                {
                    _logger.LogWarning("The operation failed to complete within the timeout. Providing a fallback value.");
                    return Task.CompletedTask;
                });

            var waitAndRetryPolicy = Policy<SentimentScoringOutcome>
                .Handle<Exception>()
                .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(100), (outcome, timeout) => 
                {
                    _logger.LogWarning("The operation failed. Retrying in {Timeout} ms", timeout.TotalMilliseconds);
                });

            return fallbackForTimeoutPolicy.WrapAsync(timeoutPolicy).WrapAsync(waitAndRetryPolicy);
        }
    }
}
