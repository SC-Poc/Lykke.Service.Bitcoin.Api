using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Pagination;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Asset
{
    public interface IAssetRepository
    {
        Task<IPaginationResult<IAsset>> GetPagedAsync(int take, int skip);
        Task<IAsset> GetByIdAsync(string assetId);
        Task<IEnumerable<IAsset>> GetColoredAssetsAsync();
        Task<IList<IAsset>> GetAllAssetsAsync();
    }
}
