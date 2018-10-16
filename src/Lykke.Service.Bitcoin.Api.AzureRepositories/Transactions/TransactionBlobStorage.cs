using System;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Transactions
{
    public class TransactionBlobStorage : ITransactionBlobStorage
    {
        private const string BlobContainer = "transactions";

        private readonly IBlobStorage _blobStorage;

        public TransactionBlobStorage(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> GetTransactionAsync(Guid operationId, string hash, TransactionBlobType type)
        {
            var key = GenerateKey(operationId, hash, type);
            if (await _blobStorage.HasBlobAsync(BlobContainer, key))
                return await _blobStorage.GetAsTextAsync(BlobContainer, key);
            return null;
        }

        public async Task AddOrReplaceTransactionAsync(Guid operationId, string hash, TransactionBlobType type,
            string transactionHex)
        {
            var key = GenerateKey(operationId, hash, type);
            if (await _blobStorage.HasBlobAsync(BlobContainer, key))
                await _blobStorage.DelBlobAsync(BlobContainer, key);
            await _blobStorage.SaveBlobAsync(BlobContainer, key, Encoding.UTF8.GetBytes(transactionHex));
        }

        private string GenerateKey(Guid operationId, string hash, TransactionBlobType type)
        {
            return $"{operationId}.{hash}.{type}.txt";
        }
    }
}
