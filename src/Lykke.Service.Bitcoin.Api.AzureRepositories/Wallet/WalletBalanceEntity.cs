using System;
using Common;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Bitcoin.Api.AzureRepositories.Wallet
{
    public class WalletBalanceEntity : TableEntity, IWalletBalance
    {
        public string Address { get; set; }
        public long Balance { get; set; }
        public DateTime Updated { get; set; }
        public int UpdatedAtBlockHeight { get; set; }
        public string AssetId { get; set; }

        public static string GeneratePartitionKey(string address)
        {
            return address.CalculateHexHash32(3);
        }

        public static string GenerateRowKey(string address, string assetId)
        {
            return address + "_" + assetId;
        }

        public static WalletBalanceEntity Create(IWalletBalance source)
        {
            return new WalletBalanceEntity
            {
                Address = source.Address,
                Balance = source.Balance,
                AssetId = source.AssetId,
                RowKey = GenerateRowKey(source.Address, source.AssetId),
                PartitionKey = GeneratePartitionKey(source.Address),
                Updated = source.Updated,
                UpdatedAtBlockHeight = source.UpdatedAtBlockHeight
            };
        }
    }
}
