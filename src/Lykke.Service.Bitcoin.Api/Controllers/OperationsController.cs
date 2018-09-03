using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Constants;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.Broadcast;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Helpers;
using Lykke.Service.Bitcoin.Api.Models;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.Bitcoin.Api.Controllers
{
    public class OperationsController : Controller
    {
        private readonly IAddressValidator _addressValidator;
        private readonly IBroadcastService _broadcastService;
        private readonly Network _network;
        private readonly IObservableOperationService _observableOperationService;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IOperationService _operationService;


        public OperationsController(IOperationService operationService,
            IAddressValidator addressValidator,
            IBroadcastService broadcastService,
            IObservableOperationService observableOperationService, IOperationEventRepository operationEventRepository,
            Network network)
        {
            _operationService = operationService;
            _addressValidator = addressValidator;
            _broadcastService = broadcastService;
            _observableOperationService = observableOperationService;
            _operationEventRepository = operationEventRepository;
            _network = network;
        }

        [HttpPost("api/transactions/single")]
        [ProducesResponseType(typeof(BuildTransactionResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> BuildSingle([FromBody] BuildSingleTransactionRequest request)
        {
            if (request == null) throw new ValidationApiException("Unable deserialize request");

            var amountSatoshi = MoneyConversionHelper.SatoshiFromContract(request.Amount);

            if (amountSatoshi <= 0)
                throw new ValidationApiException($"Amount can't be less or equal to zero: {amountSatoshi}");

            if (request.AssetId != Constants.Assets.Bitcoin.AssetId)
                throw new ValidationApiException("Invalid assetId");

            var toBitcoinAddress = _addressValidator.GetBitcoinAddress(request.ToAddress);
            if (toBitcoinAddress == null)
                throw new ValidationApiException("Invalid ToAddress ");

            var fromBitcoinAddress = _addressValidator.GetBitcoinAddress(request.FromAddress);
            if (fromBitcoinAddress == null)
                throw new ValidationApiException("Invalid FromAddress");

            if (request.OperationId == Guid.Empty)
                throw new ValidationApiException("Invalid operation id (GUID)");

            PubKey fromAddressPubkey = null;
            var pubKeyString = request.FromAddressContext?.DeserializeJson<AddressContextContract>()?.PubKey;

            if (pubKeyString != null)
            {
                if (!_addressValidator.IsPubkeyValid(pubKeyString))
                    throw new ValidationApiException("Invalid pubkey string");
                fromAddressPubkey = _addressValidator.GetPubkey(pubKeyString);
            }

            if (await _operationEventRepository.ExistAsync(request.OperationId, OperationEventType.Broadcasted))
                return Conflict();

            var tx = await _operationService.GetOrBuildTransferTransactionAsync(request.OperationId, fromBitcoinAddress,
                fromAddressPubkey,
                toBitcoinAddress,
                request.AssetId, new Money(amountSatoshi), request.IncludeFee);


            return Ok(new BuildTransactionResponse
            {
                TransactionContext = tx.ToJson(_network)
            });
        }

        [HttpPost("api/transactions/broadcast")]
        [SwaggerOperation(nameof(BroadcastTransaction))]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> BroadcastTransaction([FromBody] BroadcastTransactionRequest request)
        {
            if (request == null) throw new ValidationApiException("Unable deserialize request");

            try
            {
                await _broadcastService.BroadCastTransactionAsync(request.OperationId, request.SignedTransaction);
            }
            catch (BusinessException e) when (e.Code == ErrorCode.TransactionAlreadyBroadcasted)
            {
                return Conflict();
            }
            catch (BusinessException e) when (e.Code == ErrorCode.OperationNotFound)
            {
                return NoContent();
            }

            return Ok();
        }

        [HttpGet("api/transactions/broadcast/single/{operationId}")]
        [SwaggerOperation(nameof(GetObservableSingleOperation))]
        [ProducesResponseType(typeof(BroadcastedSingleTransactionResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> GetObservableSingleOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");

            var result = await _observableOperationService.GetByIdAsync(operationId);

            if (result == null)
                return NoContent();

            BroadcastedTransactionState MapState(BroadcastStatus status)
            {
                switch (status)
                {
                    case BroadcastStatus.Completed:
                        return BroadcastedTransactionState.Completed;
                    case BroadcastStatus.Failed:
                        return BroadcastedTransactionState.Failed;
                    case BroadcastStatus.InProgress:
                        return BroadcastedTransactionState.InProgress;
                    default:
                        throw new InvalidCastException($"Unknown mapping from {status} ");
                }
            }


            return Ok(new BroadcastedSingleTransactionResponse
            {
                Amount = MoneyConversionHelper.SatoshiToContract(result.AmountSatoshi),
                Fee = MoneyConversionHelper.SatoshiToContract(result.FeeSatoshi),
                OperationId = result.OperationId,
                Hash = result.TxHash,
                Timestamp = result.Updated,
                State = MapState(result.Status),
                Block = result.UpdatedAtBlockHeight
            });
        }

        [HttpDelete("api/transactions/broadcast/{operationId}")]
        [SwaggerOperation(nameof(RemoveObservableOperation))]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task RemoveObservableOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");
            await _observableOperationService.DeleteOperationsAsync(operationId);
        }
    }
}
