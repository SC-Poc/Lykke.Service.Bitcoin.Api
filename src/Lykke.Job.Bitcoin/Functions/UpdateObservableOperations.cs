using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Services.Operations;

namespace Lykke.Job.Bitcoin.Functions
{
    public class UpdateObservableOperations
    {
        private readonly IBlockChainProvider _blockChainProvider;
        private readonly OperationsConfirmationsSettings _confirmationsSettings;
        private readonly ILog _log;
        private readonly IObservableOperationRepository _observableOperationRepository;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IOperationMetaRepository _operationMetaRepository;
        private readonly IUnconfirmedTransactionRepository _unconfirmedTransactionRepository;
        private readonly IWalletBalanceService _walletBalanceService;

        public UpdateObservableOperations(IUnconfirmedTransactionRepository unconfirmedTransactionRepository,
            IBlockChainProvider blockChainProvider,
            IObservableOperationRepository observableOperationRepository,
            OperationsConfirmationsSettings confirmationsSettings,
            ILogFactory logFactory,
            IOperationMetaRepository operationMetaRepository,
            IOperationEventRepository operationEventRepository,
            IWalletBalanceService walletBalanceService)
        {
            _unconfirmedTransactionRepository = unconfirmedTransactionRepository;
            _blockChainProvider = blockChainProvider;
            _observableOperationRepository = observableOperationRepository;
            _confirmationsSettings = confirmationsSettings;
            _log = logFactory.CreateLog(this);
            _operationMetaRepository = operationMetaRepository;
            _operationEventRepository = operationEventRepository;
            _walletBalanceService = walletBalanceService;
        }

        [TimerTrigger("00:02:00")]
        public async Task DetectUnconfirmedTransactions()
        {
            var unconfirmedTxs = await _unconfirmedTransactionRepository.GetAllAsync();

            foreach (var unconfirmedTransaction in unconfirmedTxs)
                try
                {
                    await CheckUnconfirmedTransaction(unconfirmedTransaction);
                }
                catch (Exception e)
                {
                    _log.Error(unconfirmedTransaction.ToJson(), e);
                }
        }

        private async Task CheckUnconfirmedTransaction(IUnconfirmedTransaction unconfirmedTransaction)
        {
            var operationMeta = await _operationMetaRepository.GetAsync(unconfirmedTransaction.OperationId);
            if (operationMeta == null)
            {
                _log.Warning(unconfirmedTransaction.ToJson(), "OperationMeta not found");

                return;
            }

            var confirmationCount =
                await _blockChainProvider.GetTxConfirmationCountAsync(unconfirmedTransaction.TxHash);

            var isCompleted = confirmationCount >= _confirmationsSettings.MinConfirmationsToDetectOperation;
            ;
            if (isCompleted)
            {
                //Force update balances
                var fromAddressUpdatedBalance =
                    await _walletBalanceService.UpdateBtcBalanceAsync(operationMeta.FromAddress,
                        _confirmationsSettings.MinConfirmationsToDetectOperation);
                var toAddressUpdatedBalance = await _walletBalanceService.UpdateBtcBalanceAsync(operationMeta.ToAddress,
                    _confirmationsSettings.MinConfirmationsToDetectOperation);


                var operationCompletedLoggingContext = new
                {
                    unconfirmedTransaction.OperationId,
                    unconfirmedTransaction.TxHash,
                    fromAddressUpdatedBalance,
                    toAddressUpdatedBalance
                };

                await _operationEventRepository.InsertIfNotExistAsync(OperationEvent.Create(
                    unconfirmedTransaction.OperationId,
                    OperationEventType.DetectedOnBlockChain, operationCompletedLoggingContext));

                _log.Info(operationCompletedLoggingContext.ToJson(), "Operation completed");


                await _unconfirmedTransactionRepository.DeleteIfExistAsync(unconfirmedTransaction.OperationId);
            }

            var status = isCompleted
                ? BroadcastStatus.Completed
                : BroadcastStatus.InProgress;

            var lastBlockHeight = await _blockChainProvider.GetLastBlockHeightAsync();

            await _observableOperationRepository.InsertOrReplaceAsync(ObervableOperation.Create(operationMeta, status,
                unconfirmedTransaction.TxHash,
                lastBlockHeight));
        }
    }
}
