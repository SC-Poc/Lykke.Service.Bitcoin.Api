using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Constants;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Transactions
{
    public class HistoryService : IHistoryService
    {
        private readonly IBlockChainProvider _blockChainProvider;

        public HistoryService(IBlockChainProvider blockChainProvider)
        {
            _blockChainProvider = blockChainProvider;
        }

        public Task<IEnumerable<HistoricalTransactionDto>> GetHistoryFromAsync(BitcoinAddress address, string afterHash,
            int take)
        {
            return GetHistory(address.ToString(), afterHash, take, true);
        }

        public Task<IEnumerable<HistoricalTransactionDto>> GetHistoryToAsync(BitcoinAddress address, string afterHash,
            int take)
        {
            return GetHistory(address.ToString(), afterHash, take, false);
        }

        private async Task<IEnumerable<HistoricalTransactionDto>> GetHistory(string address, string afterHash, int take,
            bool isSending)
        {
            var result = new List<HistoricalTransactionDto>();
            var txs = await _blockChainProvider.GetTransactionsAfterTxAsync(address, afterHash);
            foreach (var tx in txs)
            {
                if (result.Count >= take)
                    break;
                var dto = MapToHistoricalTransaction(tx, address);

                if (dto.IsSending == isSending)
                    result.Add(dto);
            }

            return result;
        }


        private HistoricalTransactionDto MapToHistoricalTransaction(BitcoinTransaction tx, string requestedAddress)
        {
            var isSending = tx.Inputs.Where(p => p.Address == requestedAddress).Sum(p => p.Value) >=
                            tx.Outputs.Where(p => p.Address == requestedAddress).Sum(p => p.Value);
            string from;
            string to;
            long amount;
            if (isSending)
            {
                from = requestedAddress;
                to = tx.Outputs.Select(o => o.Address).FirstOrDefault(o => o != null && o != requestedAddress) ??
                     requestedAddress;
                amount = tx.Outputs.Where(o => o.Address != requestedAddress).Sum(o => o.Value);
            }
            else
            {
                to = requestedAddress;
                from = tx.Inputs.Select(o => o.Address).FirstOrDefault(o => o != null && o != requestedAddress) ??
                       requestedAddress;
                amount = tx.Outputs.Where(o => o.Address == requestedAddress).Sum(o => o.Value);
            }

            return new HistoricalTransactionDto
            {
                TxHash = tx.Hash,
                IsSending = isSending,
                AmountSatoshi = amount,
                FromAddress = from,
                AssetId = Constants.Assets.Bitcoin.AssetId,
                ToAddress = to,
                TimeStamp = tx.Timestamp
            };
        }
    }
}
