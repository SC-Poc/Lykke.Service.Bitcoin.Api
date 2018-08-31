using System;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.Service.Bitcoin.Api.Core.Services;

namespace Lykke.Service.Bitcoin.Api.Services.ObservableOperation
{
    public class ObservableOperationService : IObservableOperationService
    {
        private readonly IObservableOperationRepository _observableOperationRepository;
        private readonly IUnconfirmedTransactionRepository _unconfirmedTransactionRepository;

        public ObservableOperationService(IObservableOperationRepository observableOperationRepository,
            IUnconfirmedTransactionRepository unconfirmedTransactionRepository)
        {
            _observableOperationRepository = observableOperationRepository;
            _unconfirmedTransactionRepository = unconfirmedTransactionRepository;
        }

        public async Task DeleteOperationsAsync(params Guid[] opIds)
        {
            await _observableOperationRepository.DeleteIfExistAsync(opIds);
            await _unconfirmedTransactionRepository.DeleteIfExistAsync(opIds);
        }

        public Task<IObservableOperation> GetByIdAsync(Guid opId)
        {
            return _observableOperationRepository.GetByIdAsync(opId);
        }
    }
}
