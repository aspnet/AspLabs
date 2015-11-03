using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.WebHooks;

namespace CustomSender.WebHooks
{
    /// <summary>
    /// Use an <see cref="IWebHookFilterProvider"/> implementation to describe the events that users can 
    /// subscribe to. A wildcard filter is always registered meaning that users can register for 
    /// "all events". It is possible to have 0, 1, or more <see cref="IWebHookFilterProvider"/> 
    /// implementations.
    /// </summary>
    public class CustomFilterProvider : IWebHookFilterProvider
    {
        private readonly Collection<WebHookFilter> filters = new Collection<WebHookFilter>
    {
        new WebHookFilter { Name = "event1", Description = "This event happened."},
        new WebHookFilter { Name = "event2", Description = "This event happened."},
    };

        public Task<Collection<WebHookFilter>> GetFiltersAsync()
        {
            return Task.FromResult(this.filters);
        }
    }
}