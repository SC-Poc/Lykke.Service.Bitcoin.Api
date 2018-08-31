using System;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Broadcast
{
    public interface IBroadcastService
    {
        Task BroadCastTransactionAsync(Guid operationId, string txHex);
        Task BroadCastTransactionAsync(Guid operationId, Transaction transaction);
    }
}
