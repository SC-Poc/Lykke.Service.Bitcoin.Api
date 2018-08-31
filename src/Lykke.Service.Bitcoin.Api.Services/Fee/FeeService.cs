using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Fee
{
    public class FeeService : IFeeService
    {
        private readonly IFeeRateFacade _feeRateFacade;

        public FeeService(IFeeRateFacade feeRateFacade)
        {
            _feeRateFacade = feeRateFacade;
        }

        public async Task<Money> CalcFeeForTransactionAsync(Transaction tx)
        {
            var size = tx.ToBytes().Length;

            return (await GetFeeRate()).GetFee(size);
        }

        public async Task<Money> CalcFeeForTransactionAsync(TransactionBuilder builder)
        {
            var feeRate = await GetFeeRate();

            return builder.EstimateFees(builder.BuildTransaction(false), feeRate);
        }

        public async Task<FeeRate> GetFeeRate()
        {
            var feePerByte = await _feeRateFacade.GetFeePerByteAsync();

            return new FeeRate(new Money(feePerByte * 1024, MoneyUnit.Satoshi));
        }
    }
}
