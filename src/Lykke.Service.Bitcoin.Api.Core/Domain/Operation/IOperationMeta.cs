using System;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public interface IOperationMeta
    {
        Guid OperationId { get; }
        string Hash { get; }

        OperationInput[] Inputs { get; set; }
        
        OperationOutput[] Outputs { get; set; }
        
        string AssetId { get; }
        
        long FeeSatoshi { get; }

        bool IncludeFee { get; }
        DateTime Inserted { get; }
    }
}
