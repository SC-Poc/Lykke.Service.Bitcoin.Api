using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation
{
    public interface IObservableOperationRepository
    {
        Task InsertOrReplaceAsync(IObservableOperation tx);
        Task DeleteIfExistAsync(params Guid[] operationIds);
        Task<IObservableOperation> GetByIdAsync(Guid opId);
    }
}
