using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public interface IOperationEvent
    {
        OperationEventType Type { get; }

        DateTime DateTime { get; }

        Guid OperationId { get; }

        string Context { get; }
    }
}
