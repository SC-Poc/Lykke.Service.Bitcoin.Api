using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;

namespace Lykke.Job.Bitcoin.Functions
{
    public class UpdateFeeRateFunction
    {
        private readonly IFeeRateFacade _feeRateFacade;
        private readonly IFeeRateProvider _feeRateProvider;

        public UpdateFeeRateFunction(IFeeRateFacade feeRateFacade, IFeeRateProvider feeRateProvider)
        {
            _feeRateFacade = feeRateFacade;
            _feeRateProvider = feeRateProvider;
        }

        [TimerTrigger("01:00:00")]
        public async Task Update()
        {
            var rate = await _feeRateProvider.GetExternalFeeRateAsync();
            await _feeRateFacade.UpdateFeeRateAsync(rate);
        }
    }
}
