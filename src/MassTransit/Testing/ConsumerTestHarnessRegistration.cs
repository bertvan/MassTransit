namespace MassTransit.Testing
{
    using Decorators;
    using MessageObservers;
    using Registration;


    public class ConsumerTestHarnessRegistration<TConsumer> :
        IConsumerFactoryDecoratorRegistration<TConsumer>
        where TConsumer : class, IConsumer
    {
        public ConsumerTestHarnessRegistration(BusTestHarness testHarness)
        {
            Consumed = new ReceivedMessageList(testHarness.TestTimeout, testHarness.InactivityToken);
        }

        public ReceivedMessageList Consumed { get; }

        public IConsumerFactory<TConsumer> DecorateConsumerFactory(IConsumerFactory<TConsumer> consumerFactory)
        {
            return new TestConsumerFactoryDecorator<TConsumer>(consumerFactory, Consumed);
        }
    }
}
