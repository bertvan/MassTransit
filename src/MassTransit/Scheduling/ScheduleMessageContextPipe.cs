namespace MassTransit.Scheduling
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;


    public class ScheduleMessageContextPipe<T> :
        IPipe<SendContext<ScheduleMessage>>
        where T : class
    {
        readonly T _payload;
        readonly IPipe<SendContext<T>> _pipe;
        SendContext _context;

        Guid? _scheduledMessageId;

        public ScheduleMessageContextPipe(T payload, IPipe<SendContext<T>> pipe)
        {
            _payload = payload;
            _pipe = pipe;
        }

        public Guid? ScheduledMessageId
        {
            get => _context?.ScheduledMessageId ?? _scheduledMessageId;
            set => _scheduledMessageId = value;
        }

        public async Task Send(SendContext<ScheduleMessage> context)
        {
            _context = context;

            _context.ScheduledMessageId = _scheduledMessageId;

            if (_pipe.IsNotEmpty())
            {
                SendContext<T> proxy = context.CreateProxy(_payload);

                await _pipe.Send(proxy).ConfigureAwait(false);
            }
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _pipe?.Probe(context);
        }
    }
}
