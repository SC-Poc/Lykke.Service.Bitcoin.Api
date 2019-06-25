using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;
using NBitcoin.RPC;

namespace Lykke.Service.Bitcoin.Api.Services.Fee
{
    public class FeeRateProvider : IFeeRateProvider
    {
        private readonly RPCClient _rpcClient;
        private readonly int _confirmationTarget;

        public FeeRateProvider(RPCClient rpcClient, FeeRateSettings feeRateSettings)
        {
            _rpcClient = rpcClient;
            _confirmationTarget = feeRateSettings.FeeConfirmationTargetInBlocks;
        }


        public async Task<int> GetExternalFeeRateAsync()
        {
            var resp = await _rpcClient.EstimateSmartFeeAsync(_confirmationTarget);
            return (int) resp.FeeRate.SatoshiPerByte;
        }
    }
}
