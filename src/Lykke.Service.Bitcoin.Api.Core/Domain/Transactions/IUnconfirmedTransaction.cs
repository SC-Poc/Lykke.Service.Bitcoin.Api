using System;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Transactions
{
    public interface IUnconfirmedTransaction
    {
        string TxHash { get; }
        Guid OperationId { get; }
    }
}
