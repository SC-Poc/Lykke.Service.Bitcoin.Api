using Lykke.Service.Bitcoin.Api.Core.Constants;
using Lykke.Service.BlockchainApi.Contract;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Helpers
{
    public class MoneyConversionHelper
    {
        public static string SatoshiToContract(long satoshi)
        {
            return Conversions.CoinsToContract(new Money(satoshi).ToUnit(MoneyUnit.BTC),
                Constants.Assets.Bitcoin.Accuracy);
        }

        public static long SatoshiFromContract(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            var btc = Conversions.CoinsFromContract(input, Constants.Assets.Bitcoin.Accuracy);

            return new Money(btc, MoneyUnit.BTC).Satoshi;
        }
    }
}
