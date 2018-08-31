using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Transactions
{
    public interface ITransactionBuilderService
    {
        Task<IBuildedTransaction> GetTransferTransactionAsync(BitcoinAddress source, PubKey fromAddressPubkey,
            BitcoinAddress destination, Money amount, bool includeFee);
    }
}
