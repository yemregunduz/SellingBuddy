using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        public EventBusServiceBus(IServiceProvider serviceProvider, IEventBusSubscriptionManager subManager, EventBusConfig busConfig) : base(serviceProvider, subManager, busConfig)
        {
            managementClient = new ManagementClient(busConfig.EventBusConnectionString);
        }
        private ITopicClient CreateTopicClient()
        {
            if (topicClient == null || topicClient.IsClosedOrClosing)
            {
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
            }
        }

        public override void Publish(IntegrationEvent @event)
        {
            throw new NotImplementedException();
        }

        public override void Subscribe<T, TH>()
        {
            throw new NotImplementedException();
        }

        public override void UnSubscribe<T, TH>()
        {
            throw new NotImplementedException();
        }
    }
}
