using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public interface IOperationMetaRepository
    {
        Task<bool> TryInsertAsync(IOperationMeta meta);

        Task<IOperationMeta> GetAsync(Guid id);

        Task<bool> ExistAsync(Guid id);
    }
}
