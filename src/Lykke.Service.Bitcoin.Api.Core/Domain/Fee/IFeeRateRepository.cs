using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Fee
{
    public interface IFeeRateRepository
    {
        Task SetFeeRateAsync(int feePerByte);
        Task<IFeeRate> GetFeeRateAsync();
    }
}
