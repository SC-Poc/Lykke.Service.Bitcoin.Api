using System;
using AzureStorage.Tables;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Operations
{
    public class OperationMetaEntity : AzureTableEntity, IOperationMeta
    {
        public Guid OperationId { get; set; }
        public string Hash { get; set; }

        [JsonValueSerializer]
        public OperationInput[] Inputs { get; set; }

        [JsonValueSerializer]
        public OperationOutput[] Outputs { get; set; }
        
        public string AssetId { get; set; }
        public long FeeSatoshi { get; set; }
        public bool IncludeFee { get; set; }
        public DateTime Inserted { get; set; }

        public static OperationMetaEntity Map(string partitionKey, string rowKey, IOperationMeta source)
        {
            return new OperationMetaEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Hash = source.Hash,
                Inputs = source.Inputs,
                Outputs = source.Outputs,
                AssetId = source.AssetId,
                OperationId = source.OperationId,
                IncludeFee = source.IncludeFee,                
                Inserted = source.Inserted,
                FeeSatoshi = source.FeeSatoshi
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

            public static OperationMetaEntity Create(IOperationMeta source)
            {
                return Map(GeneratePartitionKey(source.OperationId), GenerateRowKey(source.OperationId), source);
            }
        }
    }
}
