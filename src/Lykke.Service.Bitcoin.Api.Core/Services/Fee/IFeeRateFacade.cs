using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Fee
{
    public interface IFeeRateFacade
    {
        Task<int> GetFeePerByteAsync();
        Task UpdateFeeRateAsync(int rate);
    }
}
