using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;

namespace Lykke.Service.Bitcoin.Api.Services.Fee
{
    public class FeeRateProvider : IFeeRateProvider
    {
        private const string Url = "https://bitcoinfees.21.co/api/v1";
        private readonly FeeType _feeType;

        public FeeRateProvider(FeeRateSettings settings)
        {
            _feeType = settings.FeeType;
        }


        public async Task<int> GetExternalFeeRateAsync()
        {
            var response = await Url.AppendPathSegment("fees/recommended").GetJsonAsync<FeeResult>();
            switch (_feeType)
            {
                case FeeType.FastestFee:
                    return response.FastestFee;
                case FeeType.HalfHourFee:
                    return response.HalfHourFee;
                default:
                    return response.HourFee;
            }
        }
    }
}
