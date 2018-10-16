using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Operations
{
    public class OperationMetaRepository : IOperationMetaRepository
    {
        private readonly INoSQLTableStorage<OperationMetaEntity> _storage;

        private OperationMetaRepository(INoSQLTableStorage<OperationMetaEntity> storage)
        {
            _storage = storage;
        }


        public Task<bool> TryInsertAsync(IOperationMeta meta)
        {
            return _storage.TryInsertAsync(OperationMetaEntity.ByOperationId.Create(meta));
        }

        public async Task<IOperationMeta> GetAsync(Guid id)
        {
            return await _storage.GetDataAsync(OperationMetaEntity.ByOperationId.GeneratePartitionKey(id),
                OperationMetaEntity.ByOperationId.GenerateRowKey(id));
        }

        public async Task<bool> ExistAsync(Guid id)
        {
            return await GetAsync(id) != null;
        }


        public static IOperationMetaRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "OperationMeta";
            var table = AzureTableStorage<OperationMetaEntity>.Create(connectionString,
                tableName, logFactory);

            return new OperationMetaRepository(table);
        }
    }
}
