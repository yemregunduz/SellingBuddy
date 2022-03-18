using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Base.Enums;
using EventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Factory
{
    public static class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig busConfig, IServiceProvider serviceProvider)
        {
            return busConfig.EventBusType switch
            {
                //we can add another MQ system. Example: AzureServiceBus
                EventBusType.RabbitMQ => new EventBusRabbitMQ(serviceProvider, busConfig),
            };
        }
    }
}
