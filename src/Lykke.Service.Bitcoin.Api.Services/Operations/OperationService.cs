using System;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;
using NBitcoin.JsonConverters;

namespace Lykke.Service.Bitcoin.Api.Services.Operations
{
    public class OperationService : IOperationService
    {
        private readonly Network _network;
        private readonly IOperationMetaRepository _operationMetaRepository;
        private readonly ITransactionBlobStorage _transactionBlobStorage;
        private readonly ITransactionBuilderService _transactionBuilder;
        private readonly ITransactionOutputsService _transactionOutputsService;

        public OperationService(ITransactionBuilderService transactionBuilder,
            IOperationMetaRepository operationMetaRepository,
            ITransactionOutputsService transactionOutputsService,
            ITransactionBlobStorage transactionBlobStorage, Network network)
        {
            _transactionBuilder = transactionBuilder;
            _operationMetaRepository = operationMetaRepository;
            _transactionOutputsService = transactionOutputsService;
            _transactionBlobStorage = transactionBlobStorage;
            _network = network;
        }

        public async Task<BuildedTransactionInfo> GetOrBuildTransferTransactionAsync(Guid operationId,
            BitcoinAddress fromAddress,
            PubKey fromAddressPubKey,
            BitcoinAddress toAddress,
            string assetId,
            Money amountToSend,
            bool includeFee)
        {
            var existingOperation = await _operationMetaRepository.GetAsync(operationId);
            if (existingOperation != null)
            {
                var existingAmount = existingOperation.IncludeFee
                    ? existingOperation.AmountSatoshi + existingOperation.FeeSatoshi
                    : existingOperation.AmountSatoshi;

                if (existingOperation.FromAddress != fromAddress.ToString() ||
                    existingOperation.ToAddress != toAddress.ToString() ||
                    existingOperation.AssetId != assetId ||
                    existingOperation.IncludeFee != includeFee ||
                    existingAmount != amountToSend.Satoshi)
                    throw new BusinessException("Conflict in operation parameters", ErrorCode.Conflict);

                return await GetExistingTransaction(existingOperation.OperationId, existingOperation.Hash);
            }

            var buildedTransaction = await _transactionBuilder.GetTransferTransactionAsync(fromAddress,
                fromAddressPubKey, toAddress,
                amountToSend, includeFee);

            var buildedTransactionInfo = new BuildedTransactionInfo
            {
                TransactionHex = buildedTransaction.TransactionData.ToHex(),
                UsedCoins = buildedTransaction.UsedCoins
            };

            await _transactionOutputsService.AddInternalOutputs(operationId,
                buildedTransaction.TransactionData.Outputs.AsCoins());

            var txHash = buildedTransaction.TransactionData.GetHash().ToString();

            await _transactionBlobStorage.AddOrReplaceTransactionAsync(operationId, txHash, TransactionBlobType.Initial,
                buildedTransactionInfo.ToJson(_network));

            var operation = OperationMeta.Create(operationId, txHash, fromAddress.ToString(), toAddress.ToString(),
                assetId,
                buildedTransaction.Amount.Satoshi, buildedTransaction.Fee.Satoshi, includeFee);

            if (await _operationMetaRepository.TryInsertAsync(operation))
                return buildedTransactionInfo;

            existingOperation = await _operationMetaRepository.GetAsync(operationId);
            return await GetExistingTransaction(operationId, existingOperation.Hash);
        }

        private async Task<BuildedTransactionInfo> GetExistingTransaction(Guid operationId, string hash)
        {
            var alreadyBuildedTransaction =
                await _transactionBlobStorage.GetTransactionAsync(operationId, hash, TransactionBlobType.Initial);
            return Serializer.ToObject<BuildedTransactionInfo>(alreadyBuildedTransaction);
        }
    }
}
