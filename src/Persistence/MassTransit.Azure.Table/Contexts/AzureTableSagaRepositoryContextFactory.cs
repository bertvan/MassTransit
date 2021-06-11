namespace MassTransit.Azure.Table.Contexts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit.Saga;
    using Microsoft.Azure.Cosmos.Table;
    using Saga;


    public class AzureTableSagaRepositoryContextFactory<TSaga> :
        ISagaRepositoryContextFactory<TSaga>
        where TSaga : class, ISaga
    {
        readonly Func<CloudTable> _databaseFactory;
        readonly ISagaConsumeContextFactory<DatabaseContext<TSaga>, TSaga> _factory;
        readonly ISagaKeyFormatter<TSaga> _keyFormatter;

        public AzureTableSagaRepositoryContextFactory(Func<CloudTable> databaseFactory,
            ISagaConsumeContextFactory<DatabaseContext<TSaga>, TSaga> factory,
            ISagaKeyFormatter<TSaga> keyFormatter)
        {
            _databaseFactory = databaseFactory;
            _factory = factory;
            _keyFormatter = keyFormatter;
        }


        public AzureTableSagaRepositoryContextFactory(CloudTable databaseFactory,
            ISagaConsumeContextFactory<DatabaseContext<TSaga>, TSaga> factory,
            ISagaKeyFormatter<TSaga> keyFormatter) :
            this(() => databaseFactory, factory, keyFormatter)
        {
        }


        public void Probe(ProbeContext context)
        {
            context.Add("persistence", "azuretable");
        }

        public async Task Send<T>(ConsumeContext<T> context, IPipe<SagaRepositoryContext<TSaga, T>> next)
            where T : class
        {
            var database = _databaseFactory();

            var databaseContext = new AzureTableDatabaseContext<TSaga>(database, _keyFormatter);

            var repositoryContext = new AzureTableSagaRepositoryContext<TSaga, T>(databaseContext, context, _factory);

            await next.Send(repositoryContext).ConfigureAwait(false);
        }

        public async Task SendQuery<T>(ConsumeContext<T> context, ISagaQuery<TSaga> query, IPipe<SagaRepositoryQueryContext<TSaga, T>> next)
            where T : class
        {
            throw new NotImplementedByDesignException("Azure Table repository does not support queries");
        }

        public async Task<T> Execute<T>(Func<SagaRepositoryContext<TSaga>, Task<T>> asyncMethod, CancellationToken cancellationToken = default)
            where T : class
        {
            var database = _databaseFactory();

            var databaseContext = new AzureTableDatabaseContext<TSaga>(database, _keyFormatter);
            var repositoryContext = new CosmosTableSagaRepositoryContext<TSaga>(databaseContext, cancellationToken);

            return await asyncMethod(repositoryContext).ConfigureAwait(false);
        }
    }
}
