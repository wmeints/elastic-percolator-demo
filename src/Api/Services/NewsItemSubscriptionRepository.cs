using Api.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public class NewsItemSubscriptionRepository: INewsItemSubscriptionRepository
    {
        private readonly ElasticClient _elasticClient;

        public NewsItemSubscriptionRepository(ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task InsertAsync(NewsItemSubscription subscription)
        {
            await _elasticClient.IndexAsync(subscription, request => request.Index("subscriptions"));
        }

        public async Task<IEnumerable<NewsItemSubscription>> GetSubscriptionsForNewsItem(NewsItem newsItem)
        {
            var response = await _elasticClient.SearchAsync<NewsItemSubscription>(
                query => query.Query(q => q.Percolate(p => p.Document(newsItem).Field(f => f.Query)))
            );

            return response.Documents;
        }
    }
}
