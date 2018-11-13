using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Logs;
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
        private readonly ILog _log;

        public UpdateBalanceFunctions(IObservableWalletRepository observableWalletRepository,
            OperationsConfirmationsSettings confirmationsSettings,
            IBlockChainProvider blockChainProvider,
            IWalletBalanceService walletBalanceService, 
            BlockHeightSettings blockHeightSettings,
            ILastProcessedBlockRepository lastProcessedBlockRepository,
            HotWalletAddressSettings hotWalletAddressSettings,
            ILogFactory logFactory)
        {
            _observableWalletRepository = observableWalletRepository;
            _confirmationsSettings = confirmationsSettings;
            _blockChainProvider = blockChainProvider;
            _walletBalanceService = walletBalanceService;
            _blockHeightSettings = blockHeightSettings;
            _lastProcessedBlockRepository = lastProcessedBlockRepository;
            _hotWalletAddressSettings = hotWalletAddressSettings;
            _log = logFactory.CreateLog(this);

            _log.Info("Starting balance updating", context: blockHeightSettings);
        }

        [TimerTrigger("00:03:00")]
        public async Task UpdateBalances()
        {
            var lastProcessedBlockHeight = await _lastProcessedBlockRepository.GetLastProcessedBlock() ?? 
                                           _blockHeightSettings.StartFromBlockHeight;
            var lastBlockHeightInBlockchain = await _blockChainProvider.GetLastBlockHeightAsync();
        
            var startFromBlock = lastProcessedBlockHeight - _confirmationsSettings.MinConfirmationsToDetectOperation * 2;
            var observableWallets = await _observableWalletRepository.GetAllAsync();

            var observableAddresses = observableWallets
                .Select(p => p.Address)
                .Where(p => p != null)
                .ToHashSet();

            for (int blockHeight = startFromBlock > 0 ? startFromBlock : 1; 
                blockHeight <= lastBlockHeightInBlockchain; 
                blockHeight++)
            {
                await ProcessBlock(blockHeight, observableAddresses);

                await _lastProcessedBlockRepository.SetLastProcessedBlock(blockHeight);
            }
        }


        private async Task ProcessBlock(int height, ISet<string> observableAddresses)
        {
            _log.Info("Processing block", context: new { Height = height });

            var txOutputAddresses = await _blockChainProvider.GetTxOutputAddresses(height);
            
            var involvedInTxAddresses = new List<string>();

            foreach (var tx in txOutputAddresses)
            {
                if (tx.destinationAddresses.Any(txOutAddress => txOutAddress == _hotWalletAddressSettings.HotWalletAddress
                                                                       || observableAddresses.Contains(txOutAddress)))
                {
                    _log.Info("Detected lykke related transaction in block", context: new { Height = height, TransactionHash = tx.txHash });
                    involvedInTxAddresses.AddRange(await _blockChainProvider.GetInvolvedInTxAddresses(tx.txHash));
                }
            }

            foreach (var address in involvedInTxAddresses
                .Where(addr => observableAddresses.Contains(addr))
                    .Distinct()) 
            {
                _log.Info("Detected lykke related address in block", context: new { Height = height, Address = address});
                await _walletBalanceService.UpdateBalanceAsync(address, _confirmationsSettings.MinConfirmationsToDetectOperation);
            }
        }
    }
}
