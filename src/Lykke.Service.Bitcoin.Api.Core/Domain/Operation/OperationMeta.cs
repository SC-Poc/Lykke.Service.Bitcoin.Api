using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public class OperationMeta : IOperationMeta
    {
        public Guid OperationId { get; set; }
        public string Hash { get; set; }      
        public OperationInput[] Inputs { get; set; }
        public OperationOutput[] Outputs { get; set; }
        public string AssetId { get; set; }
        public long FeeSatoshi { get; set; }
        public bool IncludeFee { get; set; }
        public DateTime Inserted { get; set; }

        public static OperationMeta Create(Guid operationId, string hash, IList<OperationInput> inputs, IList<OperationOutput> outputs,
            string assetId, long feeSatoshi, bool includeFee, DateTime? inserted = null)
        {
            return new OperationMeta
            {
                Hash = hash,
                Inputs = inputs.ToArray(),
                AssetId = assetId,
                Outputs = outputs.ToArray(),
                IncludeFee = includeFee,
                OperationId = operationId,                
                Inserted = inserted ?? DateTime.UtcNow,
                FeeSatoshi = feeSatoshi
            };
        }
    }
}
