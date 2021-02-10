using Api.Messages;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("/subscriptions")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly INewsItemSubscriptionManager _newsItemSubscriptionManager;
        private readonly INewsItemRepository _newsItemRepository;

        public SubscriptionsController(INewsItemSubscriptionManager newsItemSubscriptionManager, INewsItemRepository newsItemRepository)
        {
            _newsItemSubscriptionManager = newsItemSubscriptionManager;
            _newsItemRepository = newsItemRepository;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<CreateSubscriptionResponse>> CreateAsync([FromBody]CreateSubscriptionRequest request)
        {
            if(request == null)
            {
                ModelState.AddModelError("", "No query specification specified");
            }

            if(string.IsNullOrEmpty(request.Text))
            {
                ModelState.AddModelError("", "No query text specified.");
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subscription = await _newsItemSubscriptionManager.CreateAsync(request.Text);
            var newsItems = await _newsItemRepository.FindBySubscription(subscription);

            return Ok(new CreateSubscriptionResponse(subscription.Id.ToString(), newsItems));
        }
    }
}
