using System.Collections.Generic;
using NBitcoin;
using NBitcoin.JsonConverters;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Transactions
{
    public class BuiltTransactionInfo
    {
        public string TransactionHex { get; set; }

        public IEnumerable<Coin> UsedCoins { get; set; }

        public string ToJson(Network network)
        {
            return Serializer.ToString(this, network);
        }
    }
}
