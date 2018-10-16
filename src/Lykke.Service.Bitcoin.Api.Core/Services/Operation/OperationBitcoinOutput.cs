using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Operation
{
    public class OperationBitcoinOutput
    {
        public BitcoinAddress Address { get; set; }

        public Money Amount { get; set; }
    }
}
