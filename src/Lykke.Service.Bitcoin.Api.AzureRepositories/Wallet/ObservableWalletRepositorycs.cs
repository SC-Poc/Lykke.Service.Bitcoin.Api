using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Wallet
{
    public class ObservableWalletRepository : IObservableWalletRepository
    {
        private readonly INoSQLTableStorage<ObservableWalletEntity> _storage;

        private ObservableWalletRepository(INoSQLTableStorage<ObservableWalletEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertAsync(IObservableWallet wallet)
        {
            if (!await _storage.TryInsertAsync(ObservableWalletEntity.Create(wallet)))
                throw new BusinessException($"Wallet {wallet.Address} already exist", ErrorCode.EntityAlreadyExist);
        }

        public async Task<(IEnumerable<IObservableWallet>, string ContinuationToken)> GetAllAsync(int take,
            string continuationToken)
        {
            return await _storage.GetDataWithContinuationTokenAsync(take, continuationToken);
        }

        public async Task<IEnumerable<IObservableWallet>> GetAllAsync()
        {
            return await _storage.GetDataAsync();
        }

        public async Task DeleteAsync(string address)
        {
            if (!await _storage.DeleteIfExistAsync(ObservableWalletEntity.GeneratePartitionKey(address),
                ObservableWalletEntity.GenerateRowKey(address)))
                throw new BusinessException($"Wallet {address} not exist", ErrorCode.EntityNotExist);
        }

        public async Task<IObservableWallet> GetAsync(string address)
        {
            return await _storage.GetDataAsync(ObservableWalletEntity.GeneratePartitionKey(address),
                ObservableWalletEntity.GenerateRowKey(address));
        }


        public static IObservableWalletRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "ObservableWallets";
            var table = AzureTableStorage<ObservableWalletEntity>.Create(connectionString,
                tableName, logFactory);

            return new ObservableWalletRepository(table);
        }
    }
}
