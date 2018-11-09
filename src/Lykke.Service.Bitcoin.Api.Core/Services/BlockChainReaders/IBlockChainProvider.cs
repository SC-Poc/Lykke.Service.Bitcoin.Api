using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders
{
    public interface IBlockChainProvider
    {
        Task BroadCastTransactionAsync(Transaction tx);
        Task<int> GetTxConfirmationCountAsync(string txHash);
        Task<IList<Coin>> GetUnspentOutputsAsync(string address, int minConfirmationCount);
        Task<IList<ColoredCoin>> GetColoredUnspentOutputsAsync(string address, int minConfirmationCount);
        Task<long> GetBalanceSatoshiFromUnspentOutputsAsync(string address, int minConfirmationCount);
        Task<int> GetLastBlockHeightAsync();
        Task<IEnumerable<BitcoinTransaction>> GetTransactionsAfterTxAsync(string address, string afterHash);
        Task<GetTransactionResponse> GetTransactionAsync(uint256 txHash);
        Task<GetBlockResponse> GetBlockAsync(int blockHeight);
    }
}
