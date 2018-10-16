namespace Lykke.Service.Bitcoin.Api.Core.Domain.Asset
{
    public interface IAsset
    {
        string AssetId { get; }
        string Name { get; }
        int Accuracy { get; }
    }
}
