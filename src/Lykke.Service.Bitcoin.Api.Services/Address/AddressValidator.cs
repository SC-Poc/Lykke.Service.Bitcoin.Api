using System;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Address
{
    public class AddressValidator : IAddressValidator
    {
        private readonly Network _network;

        public AddressValidator(Network network)
        {
            _network = network;
        }


        public bool IsValid(string address)
        {
            var addr = GetBitcoinAddress(address);

            return addr != null;
        }


        public BitcoinAddress GetBitcoinAddress(string base58Data)
        {
            try
            {
                return BitcoinAddress.Create(base58Data, _network);
            }
            catch (Exception)
            {
                try
                {
                    return new BitcoinColoredAddress(base58Data, _network).Address;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool IsPubkeyValid(string pubkey)
        {
            return GetPubkey(pubkey) != null;
        }

        public PubKey GetPubkey(string pubkey)
        {
            try
            {
                return new PubKey(pubkey);
            }
            catch
            {
                return null;
            }
        }
    }
}
