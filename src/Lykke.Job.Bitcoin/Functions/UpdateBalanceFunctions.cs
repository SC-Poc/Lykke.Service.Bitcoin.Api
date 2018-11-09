using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Services.Operations;
using Lykke.Service.Bitcoin.Api.Services.Wallet;
using NBitcoin;

namespace Lykke.Job.Bitcoin.Functions
{
    public class UpdateBalanceFunctions
    {
        private readonly OperationsConfirmationsSettings _confirmationsSettings;
        private readonly IBlockChainProvider _blockChainProvider;
        private readonly IObservableWalletRepository _observableWalletRepository;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly BlockHeightSettings _blockHeightSettings;
        private readonly ILastProcessedBlockRepository _lastProcessedBlockRepository;
        private readonly HotWalletAddressSettings _hotWalletAddressSettings;
        private readonly Network _network;

        public UpdateBalanceFunctions(IObservableWalletRepository observableWalletRepository,
            OperationsConfirmationsSettings confirmationsSettings,
            IBlockChainProvider blockChainProvider,
            IWalletBalanceService walletBalanceService, 
            BlockHeightSettings blockHeightSettings,
            ILastProcessedBlockRepository lastProcessedBlockRepository,
            HotWalletAddressSettings hotWalletAddressSettings, 
            Network network)
        {
            _observableWalletRepository = observableWalletRepository;
            _confirmationsSettings = confirmationsSettings;
            _blockChainProvider = blockChainProvider;
            _walletBalanceService = walletBalanceService;
            _blockHeightSettings = blockHeightSettings;
            _lastProcessedBlockRepository = lastProcessedBlockRepository;
            _hotWalletAddressSettings = hotWalletAddressSettings;
            _network = network;
        }

        [TimerTrigger("00:03:00")]
        public async Task UpdateBalances()
        {
            var lastProcessedBlockHeight = await _lastProcessedBlockRepository.GetLastProcessedBlock() ?? 
                                           _blockHeightSettings.StartFromBlockHeight;
            var lastBlockHeightInBlockchain = await _blockChainProvider.GetLastBlockHeightAsync();
        
            var startFromBlock =
                lastProcessedBlockHeight - _confirmationsSettings.MinConfirmationsToDetectOperation * 2;

            for (int blockHeight = startFromBlock > 0 ? startFromBlock : 1; 
                blockHeight <= lastBlockHeightInBlockchain; 
                blockHeight++)
            {
                await ProcessBlock(blockHeight);

                await _lastProcessedBlockRepository.SetLastProcessedBlock(blockHeight);
            }
        }

        private async Task ProcessBlock(int height)
        {
            var getBlockData = _blockChainProvider.GetBlockAsync(height);
            var getObserwableWallets = _observableWalletRepository.GetAllAsync();

            await Task.WhenAll(getBlockData, getObserwableWallets);

            var observableAddresses = getObserwableWallets.Result.Select(p => p.Address)
                .Union(new[] { _hotWalletAddressSettings.HotWalletAddress })
                .Distinct()
                .Select(p => BitcoinAddress.Create(p, _network))
                .ToHashSet();
            
            foreach (var tx in getBlockData.Result.Block.Transactions)
            {
                if (tx.Outputs.AsIndexedOutputs()
                    .Any(p => observableAddresses.Contains(p.TxOut.ScriptPubKey.GetDestinationAddress(_network))))
                {
                    await UpdateBalancesOfTransactionInvolvedAddresses(tx.GetHash());
                }
            }
        }

        private async Task UpdateBalancesOfTransactionInvolvedAddresses(uint256 txHash)
        {
            var fullTxData = await _blockChainProvider.GetTransactionAsync(txHash);

            var inputAddresses = fullTxData.SpentCoins.Select(p => p.TxOut.ScriptPubKey.GetDestinationAddress(_network));
            var outputAddresses = fullTxData.ReceivedCoins.Select(p => p.TxOut.ScriptPubKey.GetDestinationAddress(_network));

            var updateAddressBalanceTasks = new List<Task>();

            foreach (var address in inputAddresses.Union(outputAddresses)
                .Distinct()
                .Where(p => p != null)) // colored address marker
            {
                updateAddressBalanceTasks.Add(_walletBalanceService.UpdateBalanceAsync(address.ToString(),
                    _confirmationsSettings.MinConfirmationsToDetectOperation));
            }

            await Task.WhenAll(updateAddressBalanceTasks);
        }
    }
}
