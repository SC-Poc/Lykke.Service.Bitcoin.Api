using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Operation
{
    public static class OperationInputExtensions
    {
        public static OperationBitcoinInput ToBitcoinInput(this OperationInput input, IAddressValidator addressValidator)
        {
            var address = addressValidator.GetBitcoinAddress(input.Address);
            if (address == null)
                throw new BusinessException("Invalid bitcoin address", ErrorCode.BadInputParameter);
            PubKey pubKey = null;
            if (!string.IsNullOrEmpty(input.AddressContext))
            {
                pubKey = addressValidator.GetPubkey(input.AddressContext);
                if (pubKey == null)
                    throw new BusinessException("Invalid pubkey", ErrorCode.BadInputParameter);
            }
            return new OperationBitcoinInput
            {
                Address = address,
                Redeem = pubKey?.WitHash.ScriptPubKey,
                Amount = new Money(input.Amount)
            };
        }
    }
}
