using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Outputs
{
    public interface IInternalOutputRepository
    {
        Task InsertOrReplaceOutputsAsync(IEnumerable<IInternalOutput> outputs);
        Task<IEnumerable<IInternalOutput>> GetOutputsAsync(string address);
        Task RemoveOutputsAsync(IEnumerable<IOutput> outputs);
        Task SetTxHashAsync(Guid operationId, string txHash);
    }
}
