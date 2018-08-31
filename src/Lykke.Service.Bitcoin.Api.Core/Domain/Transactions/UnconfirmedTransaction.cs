using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Transactions
{
    public class UnconfirmedTransaction : IUnconfirmedTransaction
    {
        public string TxHash { get; set; }
        public Guid OperationId { get; set; }

        public static UnconfirmedTransaction Create(Guid opId, string txHash)
        {
            return new UnconfirmedTransaction
            {
                OperationId = opId,
                TxHash = txHash
            };
        }
    }
}
