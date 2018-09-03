using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Outputs
{
    public interface ISpentOutputRepository
    {
        Task InsertSpentOutputsAsync(Guid transactionId, IEnumerable<IOutput> outputs);

        Task<IEnumerable<IOutput>> GetSpentOutputsAsync(IEnumerable<IOutput> outputs);

        Task RemoveOldOutputsAsync(DateTime bound);
    }
}
