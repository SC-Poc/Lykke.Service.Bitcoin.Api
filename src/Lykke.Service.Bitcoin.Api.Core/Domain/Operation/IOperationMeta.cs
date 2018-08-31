using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public interface IOperationMeta
    {
        Guid OperationId { get; }
        string Hash { get; }
        string FromAddress { get; }

        string ToAddress { get; }

        string AssetId { get; }

        long AmountSatoshi { get; }
        long FeeSatoshi { get; }

        bool IncludeFee { get; }
        DateTime Inserted { get; }
    }
}
