using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs
{
    public interface ITransactionOutputsService
    {
        Task<IEnumerable<Coin>> GetUnspentOutputsAsync(string address, int confirmationsCount = 0);
        Task AddInternalOutputs(Guid operationId, IEnumerable<Coin> coins);
        Task CompleteTxOutputs(Guid operationId, Transaction tx);
    }
}
