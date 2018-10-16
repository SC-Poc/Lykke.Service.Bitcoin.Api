using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public interface IOperationEventRepository
    {
        Task InsertIfNotExistAsync(IOperationEvent operationEvent);
        Task<bool> ExistAsync(Guid operationId, OperationEventType type);
    }
}
