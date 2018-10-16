using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Fee
{
    public interface IFeeRateProvider
    {
        Task<int> GetExternalFeeRateAsync();
    }
}
