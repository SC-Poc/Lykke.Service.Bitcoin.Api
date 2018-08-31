using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;
using Lykke.Service.Bitcoin.Api.Core.Services.Asset;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.Bitcoin.Api.Services.Asset
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly ILog _log;
        private readonly IMemoryCache _memoryCache;

        public AssetService(IAssetRepository assetRepository, ILogFactory logFactory,
            IAssetsServiceWithCache assetsServiceWithCache,
            IMemoryCache memoryCache)
        {
            _assetRepository = assetRepository;
            _assetsServiceWithCache = assetsServiceWithCache;
            _memoryCache = memoryCache;
            _log = logFactory.CreateLog(this);
        }

        public async Task<IList<ColoredAsset>> GetColoredAssetsAsync()
        {
            return await _memoryCache.GetOrCreateAsync("colored_assets", async entry =>
            {
                var coloredAssets = await _assetRepository.GetColoredAssetsAsync();

                var assets = await _assetsServiceWithCache.GetAllAssetsAsync(true);

                var result = new List<ColoredAsset>();

                foreach (var asset in coloredAssets)
                {
                    var dictAsset = assets.FirstOrDefault(o => o.BlockchainIntegrationLayerAssetId == asset.AssetId);
                    if (dictAsset == null)
                        _log.Error(null,
                            $"Asset with BlockchainIntegrationLayerAssetId={asset.AssetId} is not found. Please configure it.");
                    else
                        result.Add(new ColoredAsset
                        {
                            AssetId = asset.AssetId,
                            Accuracy = asset.Accuracy,
                            BlockchainAssetId = dictAsset.BlockChainAssetId,
                            Name = asset.Name
                        });
                }

                entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                return result;
            });
        }

        public Task<IList<IAsset>> GetAllAssetsAsync()
        {
            return _assetRepository.GetAllAssetsAsync();
        }
    }
}
