using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Asset
{
    public class Asset : IAsset
    {
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Accuracy { get; set; }
    }
}
