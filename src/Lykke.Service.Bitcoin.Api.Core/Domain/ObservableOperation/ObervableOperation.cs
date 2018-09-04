using System;
using System.Linq;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation
{
    public class ObervableOperation : IObservableOperation
    {
        public bool IncludeFee { get; set; }
        public Guid OperationId { get; set; }
        public OperationInput[] Inputs { get; set; }
        public OperationOutput[] Outputs { get; set; }        
        public string AssetId { get; set; }        
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
                Inputs = operation.Inputs.ToArray(),
                AssetId = operation.AssetId,
                Outputs = operation.Outputs.ToArray(),
                IncludeFee = operation.IncludeFee,                
                Status = status,
                TxHash = txHash,
                Updated = updated ?? DateTime.UtcNow,
                FeeSatoshi = operation.FeeSatoshi,
                UpdatedAtBlockHeight = updatedAtBlockHeight,                
            };
        }
    }
}
