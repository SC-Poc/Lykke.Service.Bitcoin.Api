using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Asset
{
    public interface IAssetService
    {
        Task<IList<ColoredAsset>> GetColoredAssetsAsync();
        Task<IList<IAsset>> GetAllAssetsAsync();
    }
}
