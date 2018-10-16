using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;

namespace Lykke.Service.Bitcoin.Api.Services.Operations
{
    public static class OperationMetaComparer
    {
        public static bool Compare(IOperationMeta operationMeta, IList<OperationInput> inputs,
            IList<OperationOutput> outputs,
            string assetId,
            bool includeFee)
        {
            if (operationMeta.Inputs.Length != inputs.Count || operationMeta.Outputs.Length != outputs.Count)
                return false;

            var orderedInputs = operationMeta.Inputs.OrderBy(o => o.Address).ThenBy(o => o.Amount).ToList();
            var orderedOtherInputs = inputs.OrderBy(o => o.Address).ThenBy(o => o.Amount).ToList();

            for (var i = 0; i < inputs.Count; i++)
                if (orderedInputs[i].Address != orderedOtherInputs[i].Address ||
                    orderedInputs[i].Amount != orderedOtherInputs[i].Amount)
                    return false;

            var orderedOutputs = operationMeta.Outputs.OrderBy(o => o.Address).ThenBy(o => o.Amount).ToList();
            var orderedOtherOutputs = outputs.OrderBy(o => o.Address).ThenBy(o => o.Amount).ToList();

            for (var i = 0; i < outputs.Count; i++)
                if (orderedOutputs[i].Address != orderedOtherOutputs[i].Address ||
                    orderedOutputs[i].Amount != orderedOtherOutputs[i].Amount)
                    return false;

            return operationMeta.AssetId == assetId && operationMeta.IncludeFee == includeFee;
        }
    }
}
