using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface INewsItemSubscriptionRepository
    {
        Task InsertAsync(NewsItemSubscription subscription);
        Task<IEnumerable<NewsItemSubscription>> GetSubscriptionsForNewsItem(NewsItem newsItem);
    }
}
