using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public class WalletBalance : IWalletBalance
    {
        public string Address { get; set; }
        public long Balance { get; set; }
        public DateTime Updated { get; set; }
        public int UpdatedAtBlockHeight { get; set; }
        public string AssetId { get; set; }

        public static WalletBalance Create(string address, long balance, int updatedAtBlock, string assetId,
            DateTime? updated = null)
        {
            return new WalletBalance
            {
                Address = address,
                AssetId = assetId,
                Balance = balance,
                Updated = updated ?? DateTime.UtcNow,
                UpdatedAtBlockHeight = updatedAtBlock
            };
        }
    }
}
