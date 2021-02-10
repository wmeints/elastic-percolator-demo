using Api.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface INewsItemSubscriptionManager
    {
        Task<NewsItemSubscription> CreateAsync(string text);
    }
}
