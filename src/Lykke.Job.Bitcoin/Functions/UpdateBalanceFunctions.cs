using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Services.Operations;
using Lykke.Service.Bitcoin.Api.Services.Wallet;

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

        public UpdateBalanceFunctions(IObservableWalletRepository observableWalletRepository,
            OperationsConfirmationsSettings confirmationsSettings,
            IBlockChainProvider blockChainProvider,
            IWalletBalanceService walletBalanceService, 
            BlockHeightSettings blockHeightSettings,
            ILastProcessedBlockRepository lastProcessedBlockRepository,
            HotWalletAddressSettings hotWalletAddressSettings)
        {
            _observableWalletRepository = observableWalletRepository;
            _confirmationsSettings = confirmationsSettings;
            _blockChainProvider = blockChainProvider;
            _walletBalanceService = walletBalanceService;
            _blockHeightSettings = blockHeightSettings;
            _lastProcessedBlockRepository = lastProcessedBlockRepository;
            _hotWalletAddressSettings = hotWalletAddressSettings;
        }

        [TimerTrigger("00:03:00")]
        public async Task UpdateBalances()
        {
            var lastProcessedBlockHeight = await _lastProcessedBlockRepository.GetLastProcessedBlock() ?? 
                                           _blockHeightSettings.StartFromBlockHeight;
            var lastBlockHeightInBlockchain = await _blockChainProvider.GetLastBlockHeightAsync();
        
            var startFromBlock = lastProcessedBlockHeight - _confirmationsSettings.MinConfirmationsToDetectOperation * 2;

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
            var getTxOutputAddr = _blockChainProvider.GetTxOutputAddresses(height);
            var getObserwableWallets = _observableWalletRepository.GetAllAsync();

            await Task.WhenAll(getTxOutputAddr, getObserwableWallets);

            var observableAddresses = getObserwableWallets.Result
                .Select(p => p.Address)
                .Where(p => p != null)
                .ToHashSet();

            var involvedInTxAddresses = new List<string>();

            foreach (var tx in getTxOutputAddr.Result)
            {
                // filter only lykke related transactions
                if (tx.destinationAddresses.Any(txOutAddress => txOutAddress == _hotWalletAddressSettings.HotWalletAddress
                                                                       || observableAddresses.Contains(txOutAddress)))
                {
                    //to get tx input we need to load full tx data
                    involvedInTxAddresses.AddRange(await _blockChainProvider.GetInvolvedInTxAddresses(tx.txHash));
                }
            }

            // filter other addresses included in tx,
            // which are not included in observable wallets, along with hot wallet address, to reduce ninja load.
            foreach (var address in involvedInTxAddresses
                .Where(addr => observableAddresses.Contains(addr))
                    .Distinct()) 
                
            {
                await _walletBalanceService.UpdateBalanceAsync(address, _confirmationsSettings.MinConfirmationsToDetectOperation);
            }
        }
    }
}
