using System;

namespace Enricher
{
    public class EnrichmentFailure
    {
        public string ErrorMessage { get; set; }
        public RawNewsItem OriginalItem { get; set; }
    }
}
