using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Operation
{
    public interface IOperationService
    {
        Task<BuiltTransactionInfo> GetOrBuildTransferTransactionAsync(Guid operationId,
            IList<OperationInput> inputs,
            IList<OperationOutput> outputs,
            string assetId,
            bool includeFee);
    }
}
