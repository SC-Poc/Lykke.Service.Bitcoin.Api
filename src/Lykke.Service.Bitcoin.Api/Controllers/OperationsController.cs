using System;
using System.Collections.Generic;
using System.Linq;
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
using Lykke.Service.Bitcoin.Api.Core.Services.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using Lykke.Service.Bitcoin.Api.Helpers;
using Lykke.Service.Bitcoin.Api.Models;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.Bitcoin.Api.Controllers
{
    public class OperationsController : Controller
    {        
        private readonly IBroadcastService _broadcastService;
        private readonly Network _network;
        private readonly IObservableOperationService _observableOperationService;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IOperationService _operationService;


        public OperationsController(IOperationService operationService,            
            IBroadcastService broadcastService,
            IObservableOperationService observableOperationService, IOperationEventRepository operationEventRepository,
            Network network)
        {
            _operationService = operationService;            
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

            if (request.OperationId == Guid.Empty)
                throw new ValidationApiException("Invalid operation id (GUID)");

            if (await _operationEventRepository.ExistAsync(request.OperationId, OperationEventType.Broadcasted))
                return Conflict();

            var tx = await _operationService.GetOrBuildTransferTransactionAsync(request.OperationId, new List<OperationInput>
                {
                    new OperationInput
                    {
                        Address = request.FromAddress,
                        AddressContext = request.FromAddressContext?.DeserializeJson<AddressContextContract>()?.PubKey,
                        Amount = amountSatoshi
                    }
                },
                new List<OperationOutput>
                {
                    new OperationOutput
                    {
                        Address = request.ToAddress,
                        Amount = amountSatoshi
                    }
                },
                OperationType.Single,
                request.AssetId, request.IncludeFee);


            return Ok(new BuildTransactionResponse
            {
                TransactionContext = tx.ToJson(_network)
            });
        }

        [HttpPost("api/transactions/many-inputs")]
        [ProducesResponseType(typeof(BuildTransactionResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> BuildTransactionManyInputs([FromBody]BuildTransactionWithManyInputsRequest request)
        {
            if (request == null) throw new ValidationApiException("Unable deserialize request");

            var inputs = request.Inputs.Select(o =>
            {
                var amountSatoshi = MoneyConversionHelper.SatoshiFromContract(o.Amount);
                if (amountSatoshi <= 0)
                    throw new ValidationApiException($"Amount can't be less or equal to zero: {amountSatoshi}");
                return new OperationInput
                {
                    Address = o.FromAddress,
                    AddressContext = o.FromAddressContext?.DeserializeJson<AddressContextContract>()?.PubKey,
                    Amount = amountSatoshi
                };
            }).ToList();

            if (request.AssetId != Constants.Assets.Bitcoin.AssetId)
                throw new ValidationApiException("Invalid assetId");

            if (request.OperationId == Guid.Empty)
                throw new ValidationApiException("Invalid operation id (GUID)");


            BuiltTransactionInfo tx;
            try
            {
                tx = await _operationService.GetOrBuildTransferTransactionAsync(request.OperationId, inputs,
                    new List<OperationOutput>
                    {
                        new OperationOutput
                        {
                            Address = request.ToAddress
                        }
                    },
                    OperationType.ManyInputs,
                    request.AssetId, true);
            }
            catch (NotEnoughFundsException)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughBalance));
            }
            catch (BusinessException e) when (e.Code != ErrorCode.NotEnoughFundsAvailable)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughBalance));
            }

            return Ok(new BuildTransactionResponse
            {
                TransactionContext = tx.ToJson(_network)
            });
        }



        [HttpPost("api/transactions/many-outputs")]
        [ProducesResponseType(typeof(BuildTransactionResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> BuildTransactionManyOutputs([FromBody]BuildTransactionWithManyOutputsRequest request)
        {
            if (request == null) throw new ValidationApiException("Unable deserialize request");

            var outputs = request.Outputs.Select(o =>
            {
                var amountSatoshi = MoneyConversionHelper.SatoshiFromContract(o.Amount);
                if (amountSatoshi <= 0)
                    throw new ValidationApiException($"Amount can't be less or equal to zero: {amountSatoshi}");
                return new OperationOutput
                {
                    Address = o.ToAddress,
                    Amount = amountSatoshi
                };
            }).ToList();

            if (request.AssetId != Constants.Assets.Bitcoin.AssetId)
                throw new ValidationApiException("Invalid assetId");

            if (request.OperationId == Guid.Empty)
                throw new ValidationApiException("Invalid operation id (GUID)");

            BuiltTransactionInfo tx;
            try
            {
                tx = await _operationService.GetOrBuildTransferTransactionAsync(request.OperationId, new List<OperationInput>
                    {
                        new OperationInput
                        {
                            Address = request.FromAddress,
                            AddressContext = request.FromAddressContext?.DeserializeJson<AddressContextContract>()?.PubKey
                        }
                    },
                    outputs, OperationType.ManyOutputs, request.AssetId, false);
            }
            catch (NotEnoughFundsException)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughBalance));
            }
            catch (BusinessException e) when (e.Code != ErrorCode.NotEnoughFundsAvailable)
            {
                return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughBalance));
            }

            return Ok(new BuildTransactionResponse
            {
                TransactionContext = tx.ToJson(_network)
            });
        }


        [HttpPost("api/transactions/broadcast")]
        [SwaggerOperation(nameof(BroadcastTransaction))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
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
        [ProducesResponseType(typeof(BroadcastedSingleTransactionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> GetObservableSingleOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");

            var result = await _observableOperationService.GetByIdAsync(operationId);

            if (result == null)
                return NoContent();



            return Ok(new BroadcastedSingleTransactionResponse
            {
                Amount = MoneyConversionHelper.SatoshiToContract(result.Inputs.Single().Amount - (result.IncludeFee ? result.FeeSatoshi : 0)),
                Fee = MoneyConversionHelper.SatoshiToContract(result.FeeSatoshi),
                OperationId = result.OperationId,
                Hash = result.TxHash,
                Timestamp = result.Updated,
                State = MapOperationState(result.Status),
                Block = result.UpdatedAtBlockHeight
            });
        }



        [HttpGet("api/transactions/broadcast/many-inputs/{operationId}")]
        [SwaggerOperation(nameof(GetObservableSingleOperation))]
        [ProducesResponseType(typeof(BroadcastedSingleTransactionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> GetObservableManyInputsOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");

            var result = await _observableOperationService.GetByIdAsync(operationId);

            if (result == null)
                return NoContent();


            return Ok(new BroadcastedTransactionWithManyInputsResponse()
            {
                Inputs = result.Inputs.Select(o => new BroadcastedTransactionInputContract
                {
                    Amount = MoneyConversionHelper.SatoshiToContract(o.Amount),
                    FromAddress = o.Address
                }).ToList(),
                
                Fee = MoneyConversionHelper.SatoshiToContract(result.FeeSatoshi),
                OperationId = result.OperationId,
                Hash = result.TxHash,
                Timestamp = result.Updated,
                State = MapOperationState(result.Status),
                Block = result.UpdatedAtBlockHeight
            });
        }



        [HttpGet("api/transactions/broadcast/many-outputs/{operationId}")]
        [SwaggerOperation(nameof(GetObservableSingleOperation))]
        [ProducesResponseType(typeof(BroadcastedSingleTransactionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> GetObservableManyOutputsOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");

            var result = await _observableOperationService.GetByIdAsync(operationId);

            if (result == null)
                return NoContent();


            return Ok(new BroadcastedTransactionWithManyOutputsResponse()
            {
                Outputs = result.Outputs.Select(o => new BroadcastedTransactionOutputContract
                {
                    Amount = MoneyConversionHelper.SatoshiToContract(o.Amount),
                    ToAddress = o.Address
                }).ToList(),                
                Fee = MoneyConversionHelper.SatoshiToContract(result.FeeSatoshi),
                OperationId = result.OperationId,
                Hash = result.TxHash,
                Timestamp = result.Updated,
                State = MapOperationState(result.Status),
                Block = result.UpdatedAtBlockHeight
            });
        }


        private BroadcastedTransactionState MapOperationState(BroadcastStatus status)
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

        [HttpDelete("api/transactions/broadcast/{operationId}")]
        [SwaggerOperation(nameof(RemoveObservableOperation))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task RemoveObservableOperation(Guid operationId)
        {
            if (operationId == Guid.Empty)
                throw new ValidationApiException("OperationId must be valid guid");
            await _observableOperationService.DeleteOperationsAsync(operationId);
        }
    }
}
