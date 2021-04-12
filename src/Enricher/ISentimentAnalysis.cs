using System;
using System.Linq;
using System.Threading.Tasks;

namespace Enricher
{
    public interface ISentimentAnalysis
    {
        Task<SentimentScoringOutcome> PredictAsync(string text);
    }
}
