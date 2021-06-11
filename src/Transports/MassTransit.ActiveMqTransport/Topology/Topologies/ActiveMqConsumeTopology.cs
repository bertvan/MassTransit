namespace MassTransit.ActiveMqTransport.Topology.Topologies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using GreenPipes;
    using MassTransit.Topology;
    using MassTransit.Topology.Topologies;
    using Specifications;


    public class ActiveMqConsumeTopology :
        ConsumeTopology,
        IActiveMqConsumeTopologyConfigurator
    {
        readonly IMessageTopology _messageTopology;
        readonly IActiveMqPublishTopology _publishTopology;
        readonly IList<IActiveMqConsumeTopologySpecification> _specifications;

        ActiveMqConsumerNameProvider _consumerNameProvider;
        string _queueNamePrefix;

        public ActiveMqConsumeTopology(IMessageTopology messageTopology, IActiveMqPublishTopology publishTopology)
        {
            _messageTopology = messageTopology;
            _publishTopology = publishTopology;

            _specifications = new List<IActiveMqConsumeTopologySpecification>();
        }

        IActiveMqMessageConsumeTopology<T> IActiveMqConsumeTopology.GetMessageTopology<T>()
        {
            return base.GetMessageTopology<T>() as IActiveMqMessageConsumeTopologyConfigurator<T>;
        }

        public void AddSpecification(IActiveMqConsumeTopologySpecification specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(specification);
        }

        public void UseBrokerFlavor(ActiveMqFlavor flavor)
        {
            if (flavor == ActiveMqFlavor.Artemis)
            {
                _consumerNameProvider = new FqqnConsumerNameProvider();
            }

            _consumerNameProvider = new ClassicConsumerNameProvider();
        }

        public void UsePrefix(string queueNamePrefix)
        {
            _queueNamePrefix = queueNamePrefix;
        }

        IActiveMqMessageConsumeTopologyConfigurator<T> IActiveMqConsumeTopologyConfigurator.GetMessageTopology<T>()
        {
            return base.GetMessageTopology<T>() as IActiveMqMessageConsumeTopologyConfigurator<T>;
        }

        public void Apply(IReceiveEndpointBrokerTopologyBuilder builder)
        {
            foreach (var specification in _specifications)
                specification.Apply(builder);

            ForEach<IActiveMqMessageConsumeTopologyConfigurator>(x => x.Apply(builder));
        }

        public void Bind(string topicName, Action<ITopicBindingConfigurator> configure = null)
        {
            if (topicName.StartsWith("VirtualTopic."))
            {
                var consumerName = $"Consumer.{{queue}}.{topicName}";

                var specification = new ConsumerConsumeTopologySpecification(topicName, consumerName);

                configure?.Invoke(specification);

                _specifications.Add(specification);
            }
            else
                _specifications.Add(new InvalidActiveMqConsumeTopologySpecification("Bind", $"Only virtual topics can be bound: {topicName}"));
        }

        public override string CreateTemporaryQueueName(string tag)
        {
            var result = base.CreateTemporaryQueueName(tag);
            var tempName = $"{_queueNamePrefix}{new string(result.Where(c => c != '.').ToArray())}";
            return tempName;
        }

        public override IEnumerable<ValidationResult> Validate()
        {
            return base.Validate().Concat(_specifications.SelectMany(x => x.Validate()));
        }

        protected override IMessageConsumeTopologyConfigurator CreateMessageTopology<T>(Type type)
        {
            var messageTopology = new ActiveMqMessageConsumeTopology<T>(_messageTopology.GetMessageTopology<T>(), _publishTopology.GetMessageTopology<T>(), _consumerNameProvider);

            OnMessageTopologyCreated(messageTopology);

            return messageTopology;
        }
    }
}
