using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Operations
{
    public class OperationEventRepository : IOperationEventRepository
    {
        private readonly INoSQLTableStorage<OperationEventTableEntity> _storage;

        private OperationEventRepository(INoSQLTableStorage<OperationEventTableEntity> storage)
        {
            _storage = storage;
        }


        public async Task InsertIfNotExistAsync(IOperationEvent operationEvent)
        {
            await _storage.CreateIfNotExistsAsync(OperationEventTableEntity.Create(operationEvent));
        }

        public async Task<bool> ExistAsync(Guid operationId, OperationEventType type)
        {
            return await _storage.GetDataAsync(OperationEventTableEntity.GeneratePartitionKey(operationId),
                       OperationEventTableEntity.GenerateRowKey(type)) != null;
        }


        public static IOperationEventRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "OperationEvents";
            var table = AzureTableStorage<OperationEventTableEntity>.Create(connectionString,
                tableName, logFactory);

            return new OperationEventRepository(table);
        }
    }
}
