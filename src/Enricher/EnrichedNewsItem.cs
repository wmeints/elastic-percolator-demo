using System;

namespace Enricher
{
    public class EnrichedNewsItem
    {
        public EnrichedNewsItem()
        {
        }

        public EnrichedNewsItem(Guid id, string title, string body, SentimentScoringOutcome sentimentScore)
        {
            Id = id;
            Title = title;
            Body = body;
            SentimentScore = sentimentScore;
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public SentimentScoringOutcome SentimentScore { get; set; }
    }
}
