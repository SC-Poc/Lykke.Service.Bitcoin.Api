using System.Collections.Generic;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Transactions
{
    public interface IBuiltTransaction
    {
        Transaction TransactionData { get; }
        Money Fee { get; }
        
        IEnumerable<Coin> UsedCoins { get; }
    }
}
