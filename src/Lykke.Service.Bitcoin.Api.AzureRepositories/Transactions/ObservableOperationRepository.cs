using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Transactions
{
    public class ObservableOperationRepository : IObservableOperationRepository
    {
        private readonly INoSQLTableStorage<ObservableOperationEntity> _storage;

        private ObservableOperationRepository(INoSQLTableStorage<ObservableOperationEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertOrReplaceAsync(IObservableOperation tx)
        {
            await _storage.InsertOrReplaceAsync(ObservableOperationEntity.ByOperationId.Create(tx));
        }

        public async Task DeleteIfExistAsync(params Guid[] operationIds)
        {
            foreach (var operationId in operationIds)
                await _storage.DeleteIfExistAsync(
                    ObservableOperationEntity.ByOperationId.GeneratePartitionKey(operationId),
                    ObservableOperationEntity.ByOperationId.GenerateRowKey(operationId));
        }

        public async Task<IObservableOperation> GetByIdAsync(Guid opId)
        {
            return await _storage.GetDataAsync(ObservableOperationEntity.ByOperationId.GeneratePartitionKey(opId),
                UnconfirmedTransactionEntity.GenerateRowKey(opId));
        }

        public static IObservableOperationRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "ObservableOperations";
            var table = AzureTableStorage<ObservableOperationEntity>.Create(connectionString,
                tableName, logFactory);

            return new ObservableOperationRepository(table);
        }
    }
}
