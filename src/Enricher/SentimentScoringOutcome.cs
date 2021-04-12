using System;
using System.Linq;

namespace Enricher
{
    public record SentimentScoringOutcome(string Sentiment, double PositiveScore, double NegativeScore, double NeutralScore);
}
