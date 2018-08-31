using NBitcoin;
using NBitcoin.DataEncoders;

namespace Lykke.Service.Bitcoin.Api.Services.BlockChainProviders.QbitNinja.Helpers
{
    public static class ScriptExtensions
    {
        public static Script ToScript(this string hex)
        {
            return Script.FromBytesUnsafe(Encoders.Hex.DecodeData(hex));
        }
    }
}
