using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Constants;
using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;
using Lykke.Service.Bitcoin.Api.Core.Services.Pagination;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Asset
{
    public class AssetRepository : IAssetRepository
    {
        private readonly IList<IAsset> _mockList = new List<IAsset>
        {
            new Asset
            {
                AssetId = Constants.Assets.Bitcoin.AssetId,
                Accuracy = Constants.Assets.Bitcoin.Accuracy,
                Name = Constants.Assets.Bitcoin.Name
            },
            new Asset
            {
                AssetId = Constants.Assets.Lkk.AssetId,
                Accuracy = Constants.Assets.Lkk.Accuracy,
                Name = Constants.Assets.Lkk.Name
            },
            new Asset
            {
                AssetId = Constants.Assets.Lkk1Y.AssetId,
                Accuracy = Constants.Assets.Lkk1Y.Accuracy,
                Name = Constants.Assets.Lkk1Y.Name
            },
            new Asset
            {
                AssetId = Constants.Assets.Tree.AssetId,
                Accuracy = Constants.Assets.Tree.Accuracy,
                Name = Constants.Assets.Tree.Name
            }
        };

        public Task<IPaginationResult<IAsset>> GetPagedAsync(int take, int skip)
        {
            var contination = take + skip >= _mockList.Count ? null : (skip + take).ToString();
            return Task.FromResult(PaginationResult<IAsset>.Create(_mockList.Skip(skip).Take(take), contination));
        }

        public Task<IAsset> GetByIdAsync(string assetId)
        {
            return Task.FromResult(_mockList.SingleOrDefault(p => p.AssetId == assetId));
        }

        public Task<IEnumerable<IAsset>> GetColoredAssetsAsync()
        {
            return Task.FromResult(_mockList.Where(o => o.AssetId != Constants.Assets.Bitcoin.AssetId));
        }

        public Task<IList<IAsset>> GetAllAssetsAsync()
        {
            return Task.FromResult(_mockList);
        }
    }
}
