using AzureStorage.Tables.Templates.Index;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.InternalOutputs
{
    internal static class IndexByTransactionHash
    {
        internal static string GenerateRowKey(int n)
        {
            return n.ToString();
        }

        internal static string GeneratePartitionKey(string txHash)
        {
            return txHash;
        }

        internal static AzureIndex Create(InternalOutputEntity internalOutput)
        {
            return AzureIndex.Create(GeneratePartitionKey(internalOutput.TransactionHash),
                GenerateRowKey(internalOutput.N), internalOutput);
        }
    }
}
