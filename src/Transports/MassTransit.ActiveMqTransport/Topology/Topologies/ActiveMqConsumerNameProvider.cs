namespace MassTransit.ActiveMqTransport.Topology.Topologies
{
    public interface ActiveMqConsumerNameProvider
    {
        string GetConsumerName(string entityName);
    }


    public class ActiveMqFlavorImplementationFactory
    {
        readonly ActiveMqFlavor _flavor;

        public ActiveMqFlavorImplementationFactory(ActiveMqFlavor flavor)
        {
            _flavor = flavor;
        }

        public ActiveMqConsumerNameProvider GetConsumerNameProvider()
        {
            if (_flavor == ActiveMqFlavor.Artemis)
            {
                return new FqqnConsumerNameProvider();
            }

            return new ClassicConsumerNameProvider();
        }
    }

    public class FqqnConsumerNameProvider : ActiveMqConsumerNameProvider
    {
        public string GetConsumerName(string entityName)
        {
            // Original:
            // return $"VirtualTopic.{entityName}::Consumer.{{queue}}.VirtualTopic.{entityName}";
            return $"{entityName}::Consumer.{{queue}}.{entityName}";
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
