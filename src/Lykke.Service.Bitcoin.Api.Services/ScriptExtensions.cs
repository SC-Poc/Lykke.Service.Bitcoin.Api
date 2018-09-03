using NBitcoin;
using NBitcoin.DataEncoders;

namespace Lykke.Service.Bitcoin.Api.Services.TransactionOutputs
{

    public static class ScriptExtensions
    {
        public static Script ToScript(this string hex)
        {
            return Script.FromBytesUnsafe(Encoders.Hex.DecodeData(hex));
        }
    }
}
