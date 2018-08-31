using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Transactions
{
    public interface IHistoryService
    {
        Task<IEnumerable<HistoricalTransactionDto>> GetHistoryFromAsync(BitcoinAddress address, string afterHash,
            int take);

        Task<IEnumerable<HistoricalTransactionDto>> GetHistoryToAsync(BitcoinAddress address, string afterHash,
            int take);
    }
}
