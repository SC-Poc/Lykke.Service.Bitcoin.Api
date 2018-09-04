using Lykke.AzureStorage.Tables;
using Lykke.Service.Bitcoin.Api.Core.Domain.Fee;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Fee
{
    public class FeeRateEntity : AzureTableEntity, IFeeRate
    {
        public int FeePerByte { get; set; }

        public static string GeneratePartitionKey()
        {
            return "FeeRate";
        }

        public static string GenerateRowKey()
        {
            return "FeeRate";
        }

        public static FeeRateEntity Create(int feePerByte)
        {
            return new FeeRateEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                FeePerByte = feePerByte
            };
        }
    }
}
