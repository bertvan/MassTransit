namespace MassTransit.ActiveMqTransport.Topology.Topologies
{
    public interface ActiveMqConsumerNameProvider
    {
        string GetConsumerName(string entityName);
    }


    public class FqqnConsumerNameProvider : ActiveMqConsumerNameProvider
    {
        public string GetConsumerName(string entityName)
        {
            return $"VirtualTopic.{entityName}::Consumer.{{queue}}.VirtualTopic.{entityName}";
        }
    }


    public class ClassicConsumerNameProvider : ActiveMqConsumerNameProvider
    {
        public string GetConsumerName(string entityName)
        {
            return $"Consumer.{{queue}}.VirtualTopic.{entityName}";
        }
    }
}
