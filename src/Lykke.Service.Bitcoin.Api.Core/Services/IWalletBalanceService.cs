using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services.Pagination;

namespace Lykke.Service.Bitcoin.Api.Core.Services
{
    public interface IWalletBalanceService
    {
        Task SubscribeAsync(string address);
        Task UnsubscribeAsync(string address);
        Task<IPaginationResult<IWalletBalance>> GetBalancesAsync(int take, string continuation);
        Task<IWalletBalance> UpdateBtcBalanceAsync(string address, int minConfirmations);
        Task UpdateBalanceAsync(string address, int minConfirmations);
    }
}
