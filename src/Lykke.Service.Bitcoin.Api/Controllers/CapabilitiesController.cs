using System.Net;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract.Common;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.Bitcoin.Api.Controllers
{
    public class CapabilitiesController : Controller
    {
        [SwaggerOperation(nameof(GetCapabilities))]
        [ProducesResponseType(typeof(CapabilitiesResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [HttpGet("api/capabilities")]
        public CapabilitiesResponse GetCapabilities()
        {
            return new CapabilitiesResponse
            {
                AreManyInputsSupported = false,
                AreManyOutputsSupported = false,
                IsTransactionsRebuildingSupported = false
            };
        }
    }
}
