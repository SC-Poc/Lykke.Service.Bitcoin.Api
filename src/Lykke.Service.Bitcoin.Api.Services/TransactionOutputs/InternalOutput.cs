using System;
using Lykke.Service.Bitcoin.Api.Core.Domain.Outputs;

namespace Lykke.Service.Bitcoin.Api.Services.TransactionOutputs
{
    public class InternalOutput : IInternalOutput
    {
        public string TransactionHash { get; set; }
        public int N { get; set; }
        public long Amount { get; set; }
        public string ScriptPubKey { get; set; }
        public string Address { get; set; }
        public Guid OperationId { get; set; }
    }
}
