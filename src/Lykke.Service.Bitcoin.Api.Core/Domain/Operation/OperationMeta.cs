using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public class OperationMeta : IOperationMeta
    {
        public Guid OperationId { get; set; }
        public string Hash { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public long AmountSatoshi { get; set; }
        public long FeeSatoshi { get; set; }
        public bool IncludeFee { get; set; }
        public DateTime Inserted { get; set; }

        public static OperationMeta Create(Guid operationId, string hash, string fromAddress, string toAddress,
            string assetId,
            long amountSatoshi, long feeSatoshi, bool includeFee, DateTime? inserted = null)
        {
            return new OperationMeta
            {
                Hash = hash,
                AmountSatoshi = amountSatoshi,
                AssetId = assetId,
                FromAddress = fromAddress,
                IncludeFee = includeFee,
                OperationId = operationId,
                ToAddress = toAddress,
                Inserted = inserted ?? DateTime.UtcNow,
                FeeSatoshi = feeSatoshi
            };
        }
    }
}
