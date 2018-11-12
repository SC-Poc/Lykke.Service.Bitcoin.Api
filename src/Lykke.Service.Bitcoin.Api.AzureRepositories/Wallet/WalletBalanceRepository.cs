﻿using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services.Pagination;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Wallet
{
    public class WalletBalanceRepository : IWalletBalanceRepository
    {
        private readonly INoSQLTableStorage<WalletBalanceEntity> _storage;

        private WalletBalanceRepository(INoSQLTableStorage<WalletBalanceEntity> storage)
        {
            _storage = storage;
        }

        public Task InsertOrReplaceAsync(IWalletBalance balance)
        {
            return _storage.InsertOrReplaceAsync(WalletBalanceEntity.Create(balance), 
                existed => existed.UpdatedAtBlockHeight <= balance.UpdatedAtBlockHeight);
        }

        public Task DeleteIfExistAsync(string address, string assetId)
        {
            return _storage.DeleteIfExistAsync(WalletBalanceEntity.GeneratePartitionKey(address),
                WalletBalanceEntity.GenerateRowKey(address, assetId));
        }

        public async Task<IPaginationResult<IWalletBalance>> GetBalancesAsync(int take, string continuation)
        {
            var result = await _storage.GetDataWithContinuationTokenAsync(take, continuation);

            return PaginationResult<IWalletBalance>.Create(result.Entities, result.ContinuationToken);
        }

        public static IWalletBalanceRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "WalletBalances";
            var table = AzureTableStorage<WalletBalanceEntity>.Create(connectionString,
                tableName, logFactory);

            return new WalletBalanceRepository(table);
        }
    }
}
