using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Wallet
{
    public class LastProcessedBlockRepository: ILastProcessedBlockRepository
    {
        private readonly INoSQLTableStorage<LastProcessedBlockEntity> _storage;

        private LastProcessedBlockRepository(INoSQLTableStorage<LastProcessedBlockEntity> storage)
        {
            _storage = storage;
        }

        public static ILastProcessedBlockRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "ObservableWallets";
            var table = AzureTableStorage<LastProcessedBlockEntity>.Create(connectionString,
                tableName, logFactory);

            return new LastProcessedBlockRepository(table);
        }

        public async Task<int?> GetLastProcessedBlock()
        {
            return (await _storage.GetDataAsync(LastProcessedBlockEntity.GeneratePartionKey(),
                LastProcessedBlockEntity.GenerateRowKey()))?.Height;
        }

        public Task SetLastProcessedBlock(int height)
        {
            return _storage.InsertOrReplaceAsync(LastProcessedBlockEntity.Create(height));
        }
    }
}
