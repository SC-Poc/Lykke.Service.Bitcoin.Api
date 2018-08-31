using System;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation
{
    public class ObervableOperation : IObservableOperation
    {
        public bool IncludeFee { get; set; }
        public Guid OperationId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public long AmountSatoshi { get; set; }
        public long FeeSatoshi { get; set; }
        public DateTime Updated { get; set; }
        public BroadcastStatus Status { get; set; }
        public string TxHash { get; set; }
        public int UpdatedAtBlockHeight { get; set; }

        public static ObervableOperation Create(IOperationMeta operation, BroadcastStatus status, string txHash,
            int updatedAtBlockHeight, DateTime? updated = null)
        {
            return new ObervableOperation
            {
                OperationId = operation.OperationId,
                AmountSatoshi = operation.AmountSatoshi,
                AssetId = operation.AssetId,
                FromAddress = operation.FromAddress,
                IncludeFee = operation.IncludeFee,
                ToAddress = operation.ToAddress,
                Status = status,
                TxHash = txHash,
                Updated = updated ?? DateTime.UtcNow,
                FeeSatoshi = operation.FeeSatoshi,
                UpdatedAtBlockHeight = updatedAtBlockHeight
            };
        }
    }
}
