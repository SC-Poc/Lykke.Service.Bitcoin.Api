using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.Bitcoin.Api.Services.Wallet
{
    public class BlockHeightSettings
    {
        public int StartFromBlockHeight { get; set; }

        public int IgnoreUnspentOutputsBeforeBlockHeight { get; set; }
    }
}
