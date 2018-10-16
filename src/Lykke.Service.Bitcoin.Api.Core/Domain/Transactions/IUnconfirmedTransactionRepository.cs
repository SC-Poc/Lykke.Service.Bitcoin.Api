using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Transactions
{
    public interface IUnconfirmedTransactionRepository
    {
        Task<IEnumerable<IUnconfirmedTransaction>> GetAllAsync();
        Task InsertOrReplaceAsync(IUnconfirmedTransaction tx);
        Task DeleteIfExistAsync(params Guid[] operationIds);
    }
}
