using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public interface IWalletBalance
    {
        string Address { get; }
        long Balance { get; }
        DateTime Updated { get; }

        int UpdatedAtBlockHeight { get; }

        string AssetId { get; }
    }
}
