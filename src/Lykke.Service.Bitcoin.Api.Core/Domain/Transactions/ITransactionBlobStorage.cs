using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Transactions
{
    public interface ITransactionBlobStorage
    {
        Task<string> GetTransactionAsync(Guid operationId, string hash, TransactionBlobType type);

        Task AddOrReplaceTransactionAsync(Guid operationId, string hash, TransactionBlobType type,
            string transactionHex);
    }
}
