using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Domain.Fee;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Fee
{
    public class FeeRateRepository : IFeeRateRepository
    {
        private readonly INoSQLTableStorage<FeeRateEntity> _storage;

        private FeeRateRepository(INoSQLTableStorage<FeeRateEntity> storage)
        {
            _storage = storage;
        }

        public Task SetFeeRateAsync(int feePerByte)
        {
            return _storage.InsertOrReplaceAsync(FeeRateEntity.Create(feePerByte));
        }

        public async Task<IFeeRate> GetFeeRateAsync()
        {
            return await _storage.GetDataAsync(FeeRateEntity.GeneratePartitionKey(), FeeRateEntity.GenerateRowKey());
        }


        public static IFeeRateRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            const string tableName = "FeeRate";
            var table = AzureTableStorage<FeeRateEntity>.Create(connectionString,
                tableName, logFactory);

            return new FeeRateRepository(table);
        }
    }
}
