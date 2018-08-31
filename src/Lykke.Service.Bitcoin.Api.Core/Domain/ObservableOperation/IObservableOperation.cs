using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation
{
    public interface IObservableOperation
    {
        BroadcastStatus Status { get; }
        Guid OperationId { get; }
        string FromAddress { get; }
        string ToAddress { get; }
        string AssetId { get; }
        long AmountSatoshi { get; }
        long FeeSatoshi { get; }
        DateTime Updated { get; }
        string TxHash { get; }
        int UpdatedAtBlockHeight { get; }
    }
}
