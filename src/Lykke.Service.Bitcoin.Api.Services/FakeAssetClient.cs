using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.Bitcoin.Api.Services
{
    //Fake everything
    public class FakeAssetClient : IAssetsServiceWithCache
    {
        public Task<IReadOnlyCollection<AssetPair>> GetAllAssetPairsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult((IReadOnlyCollection<AssetPair>)new ReadOnlyCollection<AssetPair>(new List<AssetPair>()));
        }

        public Task<IReadOnlyCollection<Assets.Client.Models.Asset>> GetAllAssetsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult((IReadOnlyCollection<Assets.Client.Models.Asset>)
            new ReadOnlyCollection<Assets.Client.Models.Asset>(new List<Assets.Client.Models.Asset>()));
        }

        public Task<IReadOnlyCollection<Assets.Client.Models.Asset>> GetAllAssetsAsync(bool includeNonTradable, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult((IReadOnlyCollection<Assets.Client.Models.Asset>)
                new ReadOnlyCollection<Assets.Client.Models.Asset>(new List<Assets.Client.Models.Asset>()));
        }

        public Task<Assets.Client.Models.Asset> TryGetAssetAsync(string assetId, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult< Assets.Client.Models.Asset>(null);
        }

        public Task<AssetPair> TryGetAssetPairAsync(string assetPairId, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<AssetPair>(null);
        }

        public Task UpdateAssetPairsCacheAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task UpdateAssetsCacheAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }
}
