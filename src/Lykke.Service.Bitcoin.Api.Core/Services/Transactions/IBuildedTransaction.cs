using System.Collections.Generic;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Transactions
{
    public interface IBuildedTransaction
    {
        Transaction TransactionData { get; }
        Money Fee { get; }
        Money Amount { get; }

        IEnumerable<Coin> UsedCoins { get; }
    }
}
