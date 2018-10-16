using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Pagination;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public interface IWalletBalanceRepository
    {
        Task InsertOrReplaceAsync(IWalletBalance balance);

        Task DeleteIfExistAsync(string address, string assetId);
        Task<IPaginationResult<IWalletBalance>> GetBalancesAsync(int take, string continuation);
    }
}
