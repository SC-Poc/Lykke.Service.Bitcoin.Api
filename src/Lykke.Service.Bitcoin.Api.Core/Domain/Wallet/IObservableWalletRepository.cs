using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public interface IObservableWalletRepository
    {
        Task InsertAsync(IObservableWallet wallet);

        Task<(IEnumerable<IObservableWallet>, string ContinuationToken)>
            GetAllAsync(int take, string continuationToken);

        Task DeleteAsync(string address);
        Task<IObservableWallet> GetAsync(string address);
    }
}
