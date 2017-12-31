using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace PusherCoreReceiver.Controllers
{
    public class PusherController : ControllerBase
    {
        private readonly ILogger _logger;

        public PusherController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PusherController>();
        }

        [PusherWebHook(Id = "It")]
        public IActionResult PusherForIt(string[] eventNames, PusherNotifications data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification creation timestamp.
            var createdAtUnix = data.CreatedAt;
            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAtUnix);
            _logger.LogInformation(
                0,
                "{ControllerName} received {Count} notifications and {EventCount} events created at '{CreatedAt}'.",
                nameof(PusherController),
                data.Events.Count,
                eventNames.Length,
                createdAt.ToString("o"));
            for (var i = 0; i < eventNames.Length; i++)
            {
                _logger.LogInformation(
                    1,
                    "Event {Index} was '{EventName}'.",
                    i,
                    eventNames[i]);
            }

            // Get details of the individual notifications.
            var index = 0;
            foreach (var @event in data.Events)
            {
                if (@event.TryGetValue(PusherConstants.EventNamePropertyName, out var eventName))
                {
                    if (@event.TryGetValue(PusherConstants.ChannelNamePropertyName, out var channelName))
                    {
                        _logger.LogInformation(
                            2,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}' and channel " +
                            "'{ChannelName}'.",
                            index,
                            @event.Count,
                            eventName,
                            channelName);
                    }
                    else
                    {
                        _logger.LogInformation(
                            3,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}'.",
                            index,
                            @event.Count,
                            eventName);
                    }
                }
                else
                {
                    _logger.LogError(
                        4,
                        "Event {EventNumber} has {Count} properties but does not contain a {PropertyName} property.",
                        index,
                        @event.Count,
                        PusherConstants.EventNamePropertyName);
                }

                index++;
            }

            return Ok();
        }

        [PusherWebHook]
        public IActionResult Pusher(string id, string[] eventNames, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification creation timestamp.
            var createdAtUnix = data.Value<long>(PusherConstants.EventRequestCreatedAtPropertyName);
            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAtUnix);
            var events = data.Value<JArray>(PusherConstants.EventRequestPropertyContainerName);
            _logger.LogInformation(
                5,
                "{ControllerName} / '{Id}' received {Count} notifications and {EventCount} events created at " +
                "'{CreatedAt}'.",
                nameof(PusherController),
                id,
                events.Count,
                eventNames.Length,
                createdAt.ToString("o"));
            for (var i = 0; i < eventNames.Length; i++)
            {
                _logger.LogInformation(
                    6,
                    "Event {Index} was '{EventName}'.",
                    i,
                    eventNames[i]);
            }

            // Get details of the individual notifications.
            var eventEnumerable = events.Values<JObject>();
            var index = 0;
            foreach (var @event in eventEnumerable)
            {
                var eventName = @event.Value<string>(PusherConstants.EventNamePropertyName);
                if (!string.IsNullOrEmpty(eventName))
                {
                    var channelName = @event.Value<string>(PusherConstants.EventNamePropertyName);
                    if (!string.IsNullOrEmpty(channelName))
                    {
                        _logger.LogInformation(
                            7,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}' and channel " +
                            "'{ChannelName}'.",
                            index,
                            @event.Count,
                            eventName,
                            channelName);
                    }
                    else
                    {
                        _logger.LogInformation(
                            8,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}'.",
                            index,
                            @event.Count,
                            eventName);
                    }
                }
                else
                {
                    _logger.LogError(
                        9,
                        "Event {EventNumber} has {Count} properties but does not contain a {PropertyName} property.",
                        index,
                        @event.Count,
                        PusherConstants.EventNamePropertyName);
                }

                index++;
            }

            return Ok();
        }
    }
}
