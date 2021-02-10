using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Messages
{
    public record CreateSubscriptionResponse(string SubscriptionId, IEnumerable<NewsItem> Items);
}
