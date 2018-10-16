using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Operation
{
    public class OperationBitcoinInput
    {
        public BitcoinAddress Address { get; set; }

        public Script Redeem { get; set; }

        public Money Amount { get; set; }
    }
}
