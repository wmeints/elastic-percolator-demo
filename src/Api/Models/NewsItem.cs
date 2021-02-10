using System;

namespace Api.Models
{
    public record NewsItem(Guid Id, string Title, string Body);
}