using System;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Transactions
{
    public class ObservableOperationEntity : AzureTableEntity, IObservableOperation
    {
        public string Status { get; set; }
        BroadcastStatus IObservableOperation.Status => Enum.Parse<BroadcastStatus>(Status);

        public Guid OperationId { get; set; }

        [JsonValueSerializer]
        public OperationInput[] Inputs { get; set; }

        [JsonValueSerializer]
        public OperationOutput[] Outputs { get; set; }
        
        public string AssetId { get; set; }
        
        public long FeeSatoshi { get; set; }
        public DateTime Updated { get; set; }
        public bool IncludeFee { get; set; }
        public string TxHash { get; set; }
        public int UpdatedAtBlockHeight { get; set; }

        public static ObservableOperationEntity Map(string partitionKey, string rowKey,
            IObservableOperation source)
        {
            return new ObservableOperationEntity
            {
                OperationId = source.OperationId,
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Inputs = source.Inputs,
                Outputs = source.Outputs,
                AssetId = source.AssetId,
                IncludeFee = source.IncludeFee,
                Status = source.Status.ToString(),
                Updated = source.Updated,
                TxHash = source.TxHash,
                FeeSatoshi = source.FeeSatoshi,
                UpdatedAtBlockHeight = source.UpdatedAtBlockHeight
            };
        }

        public static class ByOperationId
        {
            public static string GeneratePartitionKey(Guid operationId)
            {
                return operationId.ToString().CalculateHexHash32(3);
            }

            public static string GenerateRowKey(Guid operationId)
            {
                return operationId.ToString();
            }

            public static ObservableOperationEntity Create(IObservableOperation source)
            {
                return Map(GeneratePartitionKey(source.OperationId), GenerateRowKey(source.OperationId), source);
            }
        }
    }
}
