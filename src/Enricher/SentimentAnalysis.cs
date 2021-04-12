using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enricher
{
    public class SentimentAnalysis : ISentimentAnalysis
    {
        private TextAnalyticsClient _client;

        public SentimentAnalysis(TextAnalyticsOptions options)
        {
            var endpoint = new Uri(options.Endpoint);
            var credentials = new AzureKeyCredential(options.Key);

            _client = new TextAnalyticsClient(endpoint, credentials);
        }

        public async Task<SentimentScoringOutcome> PredictAsync(string text)
        {
            var results = await _client.AnalyzeSentimentAsync(text);

            return new SentimentScoringOutcome(
                results.Value.Sentiment.ToString(), 
                results.Value.ConfidenceScores.Positive, 
                results.Value.ConfidenceScores.Negative,
                results.Value.ConfidenceScores.Neutral);
        }
    }
}
