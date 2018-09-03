using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Outputs;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.InternalOutputs
{
    public class InternalOutputRepository : IInternalOutputRepository
    {
        private readonly INoSQLTableStorage<AzureIndex> _indexByAddress;
        private readonly INoSQLTableStorage<AzureIndex> _indexByTransactionHash;
        private readonly INoSQLTableStorage<InternalOutputEntity> _storage;

        private InternalOutputRepository(INoSQLTableStorage<InternalOutputEntity> storage,
            INoSQLTableStorage<AzureIndex> indexByAddress,
            INoSQLTableStorage<AzureIndex> indexByTransactionHash)
        {
            _storage = storage;
            _indexByAddress = indexByAddress;
            _indexByTransactionHash = indexByTransactionHash;
        }

        public Task InsertOrReplaceOutputsAsync(IEnumerable<IInternalOutput> outputs)
        {
            var entities = outputs.Select(InternalOutputEntity.Create);
            return _storage.InsertOrReplaceBatchAsync(entities);
        }

        public async Task<IEnumerable<IInternalOutput>> GetOutputsAsync(string address)
        {
            var indexes = await _indexByAddress.GetDataAsync(IndexByAddress.GeneratePartitionKey(address));
            return await _storage.GetDataAsync(indexes);
        }

        public async Task RemoveOutputsAsync(IEnumerable<IOutput> outputs)
        {
            foreach (var output in outputs)
            {
                var index = await _indexByTransactionHash.GetDataAsync(
                    IndexByTransactionHash.GeneratePartitionKey(output.TransactionHash),
                    IndexByTransactionHash.GenerateRowKey(output.N));
                if (index != null)
                {
                    var entity = await _storage.DeleteAsync(index);
                    if (entity != null)
                        await _indexByAddress.DeleteAsync(IndexByAddress.GeneratePartitionKey(entity.Address),
                            IndexByAddress.GenerateRowKey(entity.TransactionHash, entity.N));
                    await _indexByTransactionHash.DeleteAsync(index);
                }
            }
        }

        public async Task SetTxHashAsync(Guid operationId, string txHash)
        {
            var entities = await _storage.GetDataAsync(InternalOutputEntity.GeneratePartitionKey(operationId));
            foreach (var internalOutputEntity in entities)
            {
                var updatedEntity = await _storage.ReplaceAsync(InternalOutputEntity.GeneratePartitionKey(operationId),
                    InternalOutputEntity.GenerateRowKey(internalOutputEntity.N),
                    entity =>
                    {
                        entity.TransactionHash = txHash;
                        return entity;
                    });
                await _indexByAddress.InsertOrReplaceAsync(IndexByAddress.Create(updatedEntity));
                await _indexByTransactionHash.InsertOrReplaceAsync(IndexByTransactionHash.Create(updatedEntity));
            }
        }

        public static IInternalOutputRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "InternalOutputs";
            var table = AzureTableStorage<InternalOutputEntity>.Create(connectionString, tableName, logFactory);
            var indexByAddress = AzureTableStorage<AzureIndex>.Create(connectionString, tableName, logFactory);
            var indexByTransactionHash = AzureTableStorage<AzureIndex>.Create(connectionString, tableName, logFactory);
            return new InternalOutputRepository(table, indexByAddress, indexByTransactionHash);
        }
    }
}
