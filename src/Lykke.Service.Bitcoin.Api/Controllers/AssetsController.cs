using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.Bitcoin.Api.Controllers
{
    public class AssetsController : Controller
    {
        private readonly IAssetRepository _assetRepository;

        public AssetsController(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        [SwaggerOperation(nameof(GetPaged))]
        [ProducesResponseType(typeof(PaginationResponse<AssetResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [HttpGet("api/assets")]
        public async Task<IActionResult> GetPaged([FromQuery] int take, [FromQuery] string continuation)
        {
            if (take < 1)
                throw new ValidationApiException($"{nameof(take)} must be positive non zero integer");

            var skip = 0;
            if (!string.IsNullOrEmpty(continuation) && !int.TryParse(continuation, out skip))
                throw new ValidationApiException($"{nameof(continuation)} must be valid continuation token");

            var paginationResult = await _assetRepository.GetPagedAsync(take, skip);

            return Ok(PaginationResponse.From(paginationResult.Continuation, paginationResult.Items.Select(p =>
                new AssetResponse
                {
                    AssetId = p.AssetId,
                    Accuracy = p.Accuracy,
                    Name = p.Name
                }).ToList()));
        }

        [SwaggerOperation(nameof(GetById))]
        [ProducesResponseType(typeof(AssetResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(AssetResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [HttpGet("api/assets/{assetId}")]
        public async Task<IActionResult> GetById(string assetId)
        {
            var asset = await _assetRepository.GetByIdAsync(assetId);
            if (asset == null) return NoContent();

            return Ok(new AssetResponse
            {
                AssetId = asset.AssetId,
                Accuracy = asset.Accuracy,
                Name = asset.Name
            });
        }
    }
}
