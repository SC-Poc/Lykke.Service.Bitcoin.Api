using System;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services
{
    public interface IOperationService
    {
        Task<BuildedTransactionInfo> GetOrBuildTransferTransactionAsync(Guid operationId,
            BitcoinAddress fromAddress,
            PubKey fromAddressPubKey,
            BitcoinAddress toAddress,
            string assetId,
            Money amountToSend,
            bool includeFee);
    }
}
