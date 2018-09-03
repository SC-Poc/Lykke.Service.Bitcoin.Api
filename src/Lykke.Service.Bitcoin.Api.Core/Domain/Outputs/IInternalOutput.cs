using System;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Outputs
{
    public interface IInternalOutput : IOutput
    {
        long Amount { get; }

        string ScriptPubKey { get; }

        string Address { get; }

        Guid OperationId { get; }
    }
}
