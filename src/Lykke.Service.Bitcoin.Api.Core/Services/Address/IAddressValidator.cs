using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Address
{
    public interface IAddressValidator
    {
        bool IsValid(string address);
        BitcoinAddress GetBitcoinAddress(string address);
        bool IsPubkeyValid(string pubkey);
        PubKey GetPubkey(string pubkey);
    }
}
