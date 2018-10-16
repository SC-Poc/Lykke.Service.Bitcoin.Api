using System;

namespace Lykke.Service.Bitcoin.Api.Core.Services.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string text, ErrorCode code, Exception inner = null)
            : base(text, inner)
        {
            Code = code;
            Text = text;
        }

        public ErrorCode Code { get; }
        public string Text { get; }
    }

    public enum ErrorCode
    {
        Exception = 0,
        CantFindAddressToSignTx = 1,
        TransactionConcurrentInputsProblem = 2,
        BadInputParameter = 3,
        NotEnoughFundsAvailable = 4,
        OperationNotFound = 5,
        WalletNotFound = 7,
        SignError = 9,
        BalanceIsLessThanFee = 10,
        EntityAlreadyExist = 11,
        EntityNotExist = 12,
        TransactionAlreadyBroadcasted = 13,
        BlockChainApiError = 14,
        BroadcastError = 15,
        Conflict = 16
    }
}
