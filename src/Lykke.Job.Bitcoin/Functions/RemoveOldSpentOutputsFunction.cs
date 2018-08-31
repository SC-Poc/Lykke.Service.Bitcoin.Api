using System;
using System.Threading.Tasks;
using Lykke.Job.Bitcoin.Settings.ServiceSettings;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Domain;
using Lykke.Service.Bitcoin.Api.Services.TransactionOutputs;

namespace Lykke.Job.Bitcoin.Functions
{
    public class RemoveOldSpentOutputsFunction
    {
        private readonly SpentOutputsSettings _settings;
        private readonly ISpentOutputRepository _spentOutputRepository;

        public RemoveOldSpentOutputsFunction(ISpentOutputRepository spentOutputRepository,
            SpentOutputsSettings settings)
        {
            _spentOutputRepository = spentOutputRepository;
            _settings = settings;
        }

        [TimerTrigger("01:30:00")]
        public async Task Clean()
        {
            var bound = DateTime.UtcNow.AddDays(-_settings.SpentOutputsExpirationDays);
            await _spentOutputRepository.RemoveOldOutputsAsync(bound);
        }
    }
}
