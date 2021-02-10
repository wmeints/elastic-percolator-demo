using Api.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public class NewsItemRepository : INewsItemRepository
    {
        private readonly ElasticClient _elasticClient;

        public NewsItemRepository(ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<IEnumerable<NewsItem>> FindBySubscription(NewsItemSubscription subscription)
        {
            var response = await _elasticClient.SearchAsync<NewsItem>(request => request
                .Index("newsitems")
                .Query(x => subscription.Query)
            );

            return response.Documents;
        }

        public async Task IndexAsync(NewsItem newsItem)
        {
            var response = await _elasticClient.IndexAsync(newsItem, idx => idx.Index("newsitems"));

            if (!response.IsValid)
            {
                throw new Exception($"Failed to index news item.");
            }
        }
    }
}
