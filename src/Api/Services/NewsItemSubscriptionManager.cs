using Api.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public class NewsItemSubscriptionManager : INewsItemSubscriptionManager
    {
        private readonly INewsItemSubscriptionRepository _subscriptionRepository;


        public NewsItemSubscriptionManager(INewsItemSubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<NewsItemSubscription> CreateAsync(string text)
        {
            var subscriptionQuery = CreateQueryForSubscription(text);
            var subscription = new NewsItemSubscription(Guid.NewGuid(), subscriptionQuery);

            await _subscriptionRepository.InsertAsync(subscription);

            return subscription;
        }

        private QueryContainer CreateQueryForSubscription(string text)
        {
            var subscriptionQuery = new SimpleQueryStringQuery
            {
                Fields = new Field[]
                {
                    new Field("title"),
                    new Field("body")
                },
                Query = text,
                DefaultOperator = Operator.Or
            };

            return subscriptionQuery;
        }
    }
}
