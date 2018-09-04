using System.Collections.Generic;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Transactions
{
    public class BuiltTransaction : IBuiltTransaction
    {
        public Transaction TransactionData { get; set; }
        public Money Fee { get; set; }
        public IEnumerable<Coin> UsedCoins { get; set; }

        public static BuiltTransaction Create(Transaction transaction, Money fee, IEnumerable<Coin> usedCoins)
        {
            return new BuiltTransaction
            {
                Fee = fee,
                TransactionData = transaction,
                UsedCoins = usedCoins
            };
        }
    }
}