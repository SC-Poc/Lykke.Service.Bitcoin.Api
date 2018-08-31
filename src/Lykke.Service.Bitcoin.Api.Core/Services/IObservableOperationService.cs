using System;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;

namespace Lykke.Service.Bitcoin.Api.Core.Services
{
    public interface IObservableOperationService
    {
        Task DeleteOperationsAsync(params Guid[] opIds);
        Task<IObservableOperation> GetByIdAsync(Guid opId);
    }
}
