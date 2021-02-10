using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Api.Services
{
    public interface INewsItemEventHandler
    {
        Task HandleAsync(NewsItem item);
    }

    public class NewsItemEventHandler : INewsItemEventHandler
    {
        private readonly ILogger<NewsItemEventHandler> _logger;
        private readonly INewsItemRepository _newItemRepository;
        private readonly INewsItemSubscriptionRepository _subscriptionRepository;

        public NewsItemEventHandler(
            ILogger<NewsItemEventHandler> logger, 
            INewsItemRepository newItemRepository, 
            INewsItemSubscriptionRepository subscriptionRepository)
        {
            _logger = logger;
            _newItemRepository = newItemRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task HandleAsync(NewsItem item)
        {
            _logger.LogInformation("Received news item {Id}", item.Id);

            await IndexNewsItem(item);
            await PublishToSubscriptions(item);
        }

        private async Task IndexNewsItem(NewsItem newsItem)
        {
            _logger.LogInformation("Indexing news item {Id}", newsItem.Id);
            await _newItemRepository.IndexAsync(newsItem);
        }

        private async Task PublishToSubscriptions(NewsItem newsItem)
        {
            var subscriptions = await _subscriptionRepository.GetSubscriptionsForNewsItem(newsItem);

            _logger.LogInformation("Found {DocumentCount} subscriptions that match the news item", subscriptions.Count());

            foreach(var document in subscriptions)
            {
                _logger.LogInformation("Found subscription {SubscriptionId}", document.Id);
            }
        }
    }
}