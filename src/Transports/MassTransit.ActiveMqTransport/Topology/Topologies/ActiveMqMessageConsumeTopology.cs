namespace MassTransit.ActiveMqTransport.Topology.Topologies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using GreenPipes;
    using MassTransit.Topology.Topologies;
    using Metadata;
    using Specifications;


    public class ActiveMqMessageConsumeTopology<TMessage> :
        MessageConsumeTopology<TMessage>,
        IActiveMqMessageConsumeTopologyConfigurator<TMessage>,
        IActiveMqMessageConsumeTopologyConfigurator
        where TMessage : class
    {
        readonly string _consumerName;
        readonly IActiveMqMessagePublishTopology<TMessage> _publishTopology;
        readonly IList<IActiveMqConsumeTopologySpecification> _specifications;

        public ActiveMqMessageConsumeTopology(IActiveMqMessagePublishTopology<TMessage> publishTopology, ActiveMqConsumerNameProvider consumerNameProvider)
        {
            _publishTopology = publishTopology;

            _consumerName = consumerNameProvider.GetConsumerName(publishTopology.Topic.EntityName);

            _specifications = new List<IActiveMqConsumeTopologySpecification>();
        }

        public void Apply(IReceiveEndpointBrokerTopologyBuilder builder)
        {
            foreach (var specification in _specifications)
                specification.Apply(builder);
        }

        public void Bind(Action<ITopicBindingConfigurator> configure = null)
        {
            if (!IsBindableMessageType)
            {
                _specifications.Add(new InvalidActiveMqConsumeTopologySpecification(TypeMetadataCache<TMessage>.ShortName, "Is not a bindable message type"));
                return;
            }

            var specification = new ConsumerConsumeTopologySpecification(_publishTopology.Topic, _consumerName);

            configure?.Invoke(specification);

            _specifications.Add(specification);
        }

        public override IEnumerable<ValidationResult> Validate()
        {
            return base.Validate().Concat(_specifications.SelectMany(x => x.Validate()));
        }
    }
}
