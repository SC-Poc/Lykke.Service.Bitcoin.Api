using System;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Fee;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;

namespace Lykke.Service.Bitcoin.Api.Services.Fee
{
    public class FeeRateFacade : IFeeRateFacade
    {
        private readonly IFeeRateRepository _feeRateRepository;
        private readonly FeeRateSettings _settings;


        public FeeRateFacade(FeeRateSettings settings, IFeeRateRepository feeRateRepository)
        {
            _settings = settings;
            _feeRateRepository = feeRateRepository;
        }

        public async Task<int> GetFeePerByteAsync()
        {
            var rate = await _feeRateRepository.GetFeeRateAsync();
            if (rate == null)
                return _settings.DefaultFeeRatePerByte;

            return Math.Max(Math.Min(rate.FeePerByte, _settings.MaxFeeRatePerByte), _settings.MinFeeRatePerByte);
        }

        public Task UpdateFeeRateAsync(int rate)
        {
            return _feeRateRepository.SetFeeRateAsync(rate);
        }
    }
}
