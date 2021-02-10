using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface INewsItemRepository
    {
        Task IndexAsync(NewsItem newsItem);
        Task<IEnumerable<NewsItem>> FindBySubscription(NewsItemSubscription subscription);
    }
}
