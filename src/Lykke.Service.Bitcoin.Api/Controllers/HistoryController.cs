using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using Lykke.Service.Bitcoin.Api.Helpers;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bitcoin.Api.Controllers
{
    [Route("api/transactions/history")]
    public class HistoryController : Controller
    {
        private readonly IAddressValidator _addressValidator;
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService, IAddressValidator addressValidator)
        {
            _historyService = historyService;
            _addressValidator = addressValidator;
        }

        [HttpPost("from/{address}/observation")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public IActionResult ObserveFrom(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpPost("to/{address}/observation")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public IActionResult ObserveTo(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpDelete("from/{address}/observation")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public IActionResult DeleteObservationFrom(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpDelete("to/{address}/observation")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public IActionResult DeleteObservationTo(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpGet("from/{address}")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(HistoricalTransactionContract[]))]
        public async Task<IActionResult> GetHistoryFrom(
            [FromRoute] string address,
            [FromQuery] string afterHash,
            [FromQuery] int take)
        {
            if (take <= 0)
                return BadRequest(new ErrorResponse {ErrorMessage = $"{nameof(take)} must be greater than zero"});

            ValidateAddress(address);

            var addr = _addressValidator.GetBitcoinAddress(address);
            var result = await _historyService.GetHistoryFromAsync(addr, afterHash, take);

            return Ok(result.Select(ToHistoricalTransaction));
        }

        [HttpGet("to/{address}")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(HistoricalTransactionContract[]))]
        public async Task<IActionResult> GetHistoryTo(
            [FromRoute] string address,
            [FromQuery] string afterHash,
            [FromQuery] int take)
        {
            if (take <= 0)
                return BadRequest(new ErrorResponse {ErrorMessage = $"{nameof(take)} must be greater than zero"});

            ValidateAddress(address);

            var btcAddress = _addressValidator.GetBitcoinAddress(address);
            var result = await _historyService.GetHistoryToAsync(btcAddress, afterHash, take);

            return Ok(result.Select(ToHistoricalTransaction));
        }

        private void ValidateAddress(string address)
        {
            if (!_addressValidator.IsValid(address))
                throw new BusinessException($"Invalid BCH address ${address}", ErrorCode.BadInputParameter);
        }

        private HistoricalTransactionContract ToHistoricalTransaction(HistoricalTransactionDto source)
        {
            return new HistoricalTransactionContract
            {
                ToAddress = source.ToAddress,
                FromAddress = source.FromAddress,
                AssetId = source.AssetId,
                Amount = MoneyConversionHelper.SatoshiToContract(source.AmountSatoshi),
                Hash = source.TxHash,
                Timestamp = source.TimeStamp,
                TransactionType = source.IsSending ? TransactionType.Send : TransactionType.Receive
            };
        }
    }
}
