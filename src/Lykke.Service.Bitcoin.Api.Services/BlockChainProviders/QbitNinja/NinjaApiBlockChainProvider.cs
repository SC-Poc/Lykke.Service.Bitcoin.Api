using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.Bitcoin.Api.Services.BlockChainProviders.QbitNinja
{
    public class NinjaApiBlockChainProvider : IBlockChainProvider
    {
        private readonly IAddressValidator _addressValidator;
        private readonly Network _network;
        private readonly QBitNinjaClient _ninjaClient;

        public NinjaApiBlockChainProvider(QBitNinjaClient ninjaClient, Network network,
            IAddressValidator addressValidator)
        {
            _ninjaClient = ninjaClient;
            _ninjaClient.Colored = true;
            _network = network;
            _addressValidator = addressValidator;
        }

        public async Task BroadCastTransactionAsync(Transaction tx)
        {
            var tryCnt = 5;
            while (true)
            {
                var response = await _ninjaClient.Broadcast(tx);
                if (response.Success)
                    return;
                if (tryCnt-- <= 0 || response.Error.Reason != "Unknown")
                    throw new BusinessException(response.Error.Reason, ErrorCode.BroadcastError);
                await Task.Delay(5000);
            }
        }

        public async Task<int> GetTxConfirmationCountAsync(string txHash)
        {
            var tx = await _ninjaClient.GetTransaction(uint256.Parse(txHash));
            return tx.Block?.Confirmations ?? 0;
        }


        public async Task<IList<Coin>> GetUnspentOutputsAsync(string address, int minConfirmationCount)
        {
            return (await GetAllUnspentOutputs(address, minConfirmationCount)).OfType<Coin>().ToList();
        }

        public async Task<IList<ColoredCoin>> GetColoredUnspentOutputsAsync(string address, int minConfirmationCount)
        {
            return (await GetAllUnspentOutputs(address, minConfirmationCount)).OfType<ColoredCoin>().ToList();
        }


        public async Task<long> GetBalanceSatoshiFromUnspentOutputsAsync(string address, int minConfirmationCount)
        {
            var unspentOutputs = await GetUnspentOutputsAsync(address, minConfirmationCount);
            return unspentOutputs.Select(o => o.Amount).DefaultIfEmpty().Sum(p => p?.Satoshi ?? 0);
        }

        public async Task<int> GetLastBlockHeightAsync()
        {
            var block = await _ninjaClient.GetBlock(BlockFeature.Parse("tip"), true);
            return block.AdditionalInformation.Height;
        }


        public async Task<IEnumerable<BitcoinTransaction>> GetTransactionsAfterTxAsync(string address, string afterHash)
        {
            var heightTo = 0;
            if (!string.IsNullOrEmpty(afterHash))
            {
                var tx = await _ninjaClient.GetTransaction(uint256.Parse(afterHash));
                heightTo = tx.Block.Height;
            }

            var operations = await _ninjaClient.GetBalanceBetween(new BalanceSelector(address),
                BlockFeature.Parse("tip"), new BlockFeature(heightTo));

            return await operations.Operations.SelectAsync(async op =>
            {
                var tx = await _ninjaClient.GetTransaction(op.TransactionId);
                return new BitcoinTransaction
                {
                    Hash = op.TransactionId.ToString(),
                    Timestamp = tx.FirstSeen.DateTime,
                    Inputs = tx.SpentCoins.Select(i => new BitcoinInput
                    {
                        Address = i.TxOut.ScriptPubKey.GetDestinationAddress(_network)?.ToString(),
                        Value = i.TxOut.Value
                    }).ToList(),
                    Outputs = tx.ReceivedCoins.Select(output => new BitcoinOutput
                    {
                        Address = output.TxOut.ScriptPubKey.GetDestinationAddress(_network)?.ToString(),
                        Value = output.TxOut.Value
                    }).ToList()
                };
            });
        }

        private async Task<IList<ICoin>> GetAllUnspentOutputs(string address, int minConfirmationCount)
        {
            var response = await _ninjaClient.GetBalance(_addressValidator.GetBitcoinAddress(address), true);
            return response.Operations.Where(o => o.Confirmations >= minConfirmationCount)
                .SelectMany(o => o.ReceivedCoins).ToList();
        }
    }
}
