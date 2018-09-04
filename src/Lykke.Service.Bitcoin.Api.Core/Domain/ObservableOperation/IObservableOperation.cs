using System;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation
{
    public interface IObservableOperation
    {
        BroadcastStatus Status { get; }
        Guid OperationId { get; }
        OperationInput[] Inputs { get; set; }
        OperationOutput[] Outputs { get; set; }
        string AssetId { get; }
        long FeeSatoshi { get; }
        DateTime Updated { get; }
        bool IncludeFee { get; }
        string TxHash { get; }
        int UpdatedAtBlockHeight { get; }
    }
}
