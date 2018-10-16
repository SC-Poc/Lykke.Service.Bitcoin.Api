using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Fee
{
    public interface IFeeService
    {
        Task<Money> CalcFeeForTransactionAsync(Transaction tx);
        Task<Money> CalcFeeForTransactionAsync(TransactionBuilder builder);
    }
}
