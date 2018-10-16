using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Operation
{
    public static class OperationExtensions
    {
        public static OperationBitcoinOutput ToBitcoinOutput(this OperationOutput output, IAddressValidator addressValidator)
        {
            var address = addressValidator.GetBitcoinAddress(output.Address);
            if (address == null)
                throw new BusinessException("Invalid bitcoin address", ErrorCode.BadInputParameter);
            return new OperationBitcoinOutput
            {
                Address = address,
                Amount = new Money(output.Amount)
            };
        }

        public static long GetTransferedAmount(this IObservableOperation operation)
        {
            return operation.Outputs.Sum(o => o.Amount);
        }
    }
}
