using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Transactions
{
    public class UnconfirmedTransactionRepository : IUnconfirmedTransactionRepository
    {
        private readonly INoSQLTableStorage<UnconfirmedTransactionEntity> _storage;

        private UnconfirmedTransactionRepository(INoSQLTableStorage<UnconfirmedTransactionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<IUnconfirmedTransaction>> GetAllAsync()
        {
            return await _storage.GetDataAsync();
        }

        public Task InsertOrReplaceAsync(IUnconfirmedTransaction tx)
        {
            return _storage.InsertOrReplaceAsync(UnconfirmedTransactionEntity.Create(tx));
        }

        public async Task DeleteIfExistAsync(Guid[] operationIds)
        {
            foreach (var operationId in operationIds)
                await _storage.DeleteIfExistAsync(UnconfirmedTransactionEntity.GeneratePartitionKey(operationId),
                    UnconfirmedTransactionEntity.GenerateRowKey(operationId));
        }

        public static IUnconfirmedTransactionRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "UnconfirmedTransactions";
            var table = AzureTableStorage<UnconfirmedTransactionEntity>.Create(connectionString,
                tableName, logFactory);

            return new UnconfirmedTransactionRepository(table);
        }
    }
}
