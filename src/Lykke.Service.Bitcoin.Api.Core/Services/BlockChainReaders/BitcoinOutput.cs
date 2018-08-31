using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders
{
    public class BitcoinOutput
    {
        public string Address { get; set; }

        public Money Value { get; set; }
    }
}
