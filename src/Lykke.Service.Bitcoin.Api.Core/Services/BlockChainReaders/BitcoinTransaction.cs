using System;
using System.Collections.Generic;

namespace Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders
{
    public class BitcoinTransaction
    {
        public string Hash { get; set; }

        public DateTime Timestamp { get; set; }

        public IList<BitcoinInput> Inputs { get; set; }

        public IList<BitcoinOutput> Outputs { get; set; }
    }
}
