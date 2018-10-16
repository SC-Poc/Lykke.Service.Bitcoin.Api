using System;
using Common;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public class OperationEvent : IOperationEvent
    {
        public OperationEventType Type { get; set; }
        public DateTime DateTime { get; set; }
        public Guid OperationId { get; set; }
        public string Context { get; set; }

        public static OperationEvent Create(Guid operationId, OperationEventType type, object context = null,
            DateTime? dateTime = null)
        {
            return new OperationEvent
            {
                DateTime = dateTime ?? DateTime.UtcNow,
                OperationId = operationId,
                Type = type,
                Context = context?.ToJson()
            };
        }
    }
}
