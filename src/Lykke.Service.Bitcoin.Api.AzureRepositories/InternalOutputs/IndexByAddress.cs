using AzureStorage.Tables.Templates.Index;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.InternalOutputs
{
    internal static class IndexByAddress
    {
        internal static string GenerateRowKey(string transactionHash, int n)
        {
            return $"{transactionHash}_{n}";
        }

        internal static string GeneratePartitionKey(string address)
        {
            return address;
        }

        internal static AzureIndex Create(InternalOutputEntity internalOutput)
        {
            return AzureIndex.Create(GeneratePartitionKey(internalOutput.Address),
                GenerateRowKey(internalOutput.TransactionHash, internalOutput.N), internalOutput);
        }
    }
}
