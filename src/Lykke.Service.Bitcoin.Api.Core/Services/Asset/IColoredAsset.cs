using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Asset
{
    public class ColoredAsset : IAsset
    {
        public string BlockchainAssetId { get; set; }
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Accuracy { get; set; }
    }
}
