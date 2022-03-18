using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubManager;

        public EventBusConfig EventBusConfig { get; set; }

        protected BaseEventBus(IServiceProvider serviceProvider,EventBusConfig busConfig)
        {
            EventBusConfig = busConfig;
            ServiceProvider = serviceProvider;
            SubManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }
        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray());
            }
            if (EventBusConfig.DeleteEventSuffix)
            {
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());
            }
            return eventName;
        }
        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }
        public virtual void Dispose()
        {
            EventBusConfig = null;
        }
        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);
            var procesed = false;
            if (SubManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = SubManager.GetHandlersForEvent(eventName);
                using (var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;

                        var eventType = SubManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }
                procesed = true;

            }
            return procesed;
        }
        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
