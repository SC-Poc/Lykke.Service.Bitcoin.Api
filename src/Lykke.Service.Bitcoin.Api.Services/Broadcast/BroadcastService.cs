using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Outputs;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Core.Services.Broadcast;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using NBitcoin;
using NBitcoin.RPC;

namespace Lykke.Service.Bitcoin.Api.Services.Broadcast
{
    public class BroadcastService : IBroadcastService
    {
        private readonly IBlockChainProvider _blockChainProvider;
        private readonly ILog _log;
        private readonly Network _network;
        private readonly IObservableOperationRepository _observableOperationRepository;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IOperationMetaRepository _operationMetaRepository;
        private readonly ISpentOutputRepository _spentOutputRepository;
        private readonly ITransactionBlobStorage _transactionBlobStorage;
        private readonly ITransactionOutputsService _transactionOutputsService;
        private readonly IUnconfirmedTransactionRepository _unconfirmedTransactionRepository;

        public BroadcastService(IBlockChainProvider blockChainProvider,
            ILogFactory logFactory,
            IUnconfirmedTransactionRepository unconfirmedTransactionRepository,
            IOperationMetaRepository operationMetaRepository,
            IOperationEventRepository operationEventRepository,
            IObservableOperationRepository observableOperationRepository,
            ITransactionBlobStorage transactionBlobStorage,
            ITransactionOutputsService transactionOutputsService,
            Network network,
            ISpentOutputRepository spentOutputRepository)
        {
            _blockChainProvider = blockChainProvider;
            _log = logFactory.CreateLog(this);
            _unconfirmedTransactionRepository = unconfirmedTransactionRepository;
            _operationMetaRepository = operationMetaRepository;
            _operationEventRepository = operationEventRepository;
            _observableOperationRepository = observableOperationRepository;
            _transactionBlobStorage = transactionBlobStorage;
            _transactionOutputsService = transactionOutputsService;
            _network = network;
            _spentOutputRepository = spentOutputRepository;
        }

        public async Task BroadCastTransactionAsync(Guid operationId, Transaction tx)
        {
            var operation = await _operationMetaRepository.GetAsync(operationId);
            if (operation == null) throw new BusinessException("Operation not found", ErrorCode.BadInputParameter);

            if (await _operationEventRepository.ExistAsync(operationId, OperationEventType.Broadcasted))
                throw new BusinessException("Transaction already brodcasted", ErrorCode.TransactionAlreadyBroadcasted);
            var hash = tx.GetHash().ToString();
            await _transactionBlobStorage.AddOrReplaceTransactionAsync(operationId, hash,
                TransactionBlobType.BeforeBroadcast, tx.ToHex());

            var lastBlockHeight = await _blockChainProvider.GetLastBlockHeightAsync();

            await _blockChainProvider.BroadCastTransactionAsync(tx);

            await _observableOperationRepository.InsertOrReplaceAsync(ObervableOperation.Create(operation,
                BroadcastStatus.InProgress, hash, lastBlockHeight));

            await _unconfirmedTransactionRepository.InsertOrReplaceAsync(
                UnconfirmedTransaction.Create(operationId, hash));

            await _transactionOutputsService.CompleteTxOutputs(operationId, tx);

            await _operationEventRepository.InsertIfNotExistAsync(OperationEvent.Create(operationId,
                OperationEventType.Broadcasted));
        }

        public async Task BroadCastTransactionAsync(Guid operationId, string txHex)
        {
            Transaction tx;

            try
            {
                tx = Transaction.Parse(txHex, _network);
            }
            catch (Exception e)
            {
                _log.Error(txHex, e);
                throw new BusinessException("Invalid transaction transactionContext", ErrorCode.BadInputParameter);
            }

            await BroadCastTransactionAsync(operationId, tx);
        }
    }
}
