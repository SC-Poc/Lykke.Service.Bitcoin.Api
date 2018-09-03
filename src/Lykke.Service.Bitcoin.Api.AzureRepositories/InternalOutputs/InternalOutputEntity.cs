using System;
using Lykke.Service.Bitcoin.Api.Core.Domain.Outputs;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.InternalOutputs
{
    public class InternalOutputEntity : TableEntity, IInternalOutput
    {
        public string TransactionHash { get; set; }
        public int N { get; set; }
        public long Amount { get; set; }
        public string ScriptPubKey { get; set; }
        public string Address { get; set; }
        public Guid OperationId { get; set; }

        public static string GenerateRowKey(int n)
        {
            return n.ToString();
        }

        public static string GeneratePartitionKey(Guid operationId)
        {
            return operationId.ToString();
        }

        public static InternalOutputEntity Create(IInternalOutput internalOutput)
        {
            return new InternalOutputEntity
            {
                PartitionKey = GeneratePartitionKey(internalOutput.OperationId),
                RowKey = GenerateRowKey(internalOutput.N),
                OperationId = internalOutput.OperationId,
                Address = internalOutput.Address,
                TransactionHash = internalOutput.TransactionHash,
                N = internalOutput.N,
                Amount = internalOutput.Amount,
                ScriptPubKey = internalOutput.ScriptPubKey
            };
        }
    }
}
