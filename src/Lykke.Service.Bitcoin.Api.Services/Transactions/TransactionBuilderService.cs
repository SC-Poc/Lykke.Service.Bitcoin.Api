using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Transactions
{
    public class BuildedTransaction : IBuildedTransaction
    {
        public Transaction TransactionData { get; set; }
        public Money Fee { get; set; }
        public Money Amount { get; set; }
        public IEnumerable<Coin> UsedCoins { get; set; }

        public static BuildedTransaction Create(Transaction transaction, Money fee, Money amount,
            IEnumerable<Coin> usedCoins)
        {
            return new BuildedTransaction
            {
                Amount = amount,
                Fee = fee,
                TransactionData = transaction,
                UsedCoins = usedCoins
            };
        }
    }

    public class TransactionBuilderService : ITransactionBuilderService
    {
        private readonly IFeeService _feeService;
        private readonly ITransactionOutputsService _transactionOutputsService;

        public TransactionBuilderService(ITransactionOutputsService transactionOutputsService,
            IFeeService feeService)
        {
            _transactionOutputsService = transactionOutputsService;
            _feeService = feeService;
        }

        public async Task<IBuildedTransaction> GetTransferTransactionAsync(BitcoinAddress source,
            PubKey fromAddressPubkey,
            BitcoinAddress destination, Money amount, bool includeFee)
        {
            var builder = new TransactionBuilder();

            return await TransferOneDirection(builder, source, fromAddressPubkey, amount.Satoshi, destination,
                includeFee);
        }

        private async Task<IBuildedTransaction> TransferOneDirection(TransactionBuilder builder,
            BitcoinAddress from, PubKey fromAddressPubkey, long amount, BitcoinAddress to, bool includeFee)
        {
            var fromStr = from.ToString();
            var coins = (await _transactionOutputsService.GetUnspentOutputsAsync(fromStr)).ToList();

            if (fromAddressPubkey != null)
            {
                var reedem = fromAddressPubkey.WitHash.ScriptPubKey;
                coins = coins.Select(p => p.ToScriptCoin(reedem)).Cast<Coin>().ToList();
            }

            var balance = coins.Select(o => o.Amount).Sum(o => o.Satoshi);

            if (balance > amount &&
                balance - amount < new TxOut(Money.Zero, from)
                    .GetDustThreshold(builder.StandardTransactionPolicy.MinRelayTxFee).Satoshi)
                amount = balance;

            return await SendWithChange(builder, coins, to, new Money(balance), new Money(amount),
                from, includeFee);
        }

        public async Task<IBuildedTransaction> SendWithChange(TransactionBuilder builder, List<Coin> coins,
            IDestination destination, Money balance, Money amount, IDestination changeDestination, bool includeFee)
        {
            if (amount.Satoshi <= 0)
                throw new BusinessException("Amount can't be less or equal to zero", ErrorCode.BadInputParameter);

            builder.AddCoins(coins)
                .Send(destination, amount)
                .SetChange(changeDestination);

            var calculatedFee = await _feeService.CalcFeeForTransactionAsync(builder);
            var requiredBalance = amount + (includeFee ? Money.Zero : calculatedFee);

            if (balance < requiredBalance)
                throw new BusinessException(
                    $"The sum of total applicable outputs is less than the required : {requiredBalance} satoshis.",
                    ErrorCode.NotEnoughFundsAvailable);

            if (includeFee)
            {
                if (calculatedFee > amount)
                    throw new BusinessException(
                        $"The sum of total applicable outputs is less than the required fee:{calculatedFee} satoshis.",
                        ErrorCode.BalanceIsLessThanFee);
                builder.SubtractFees();
                amount = amount - calculatedFee;
            }

            builder.SendFees(calculatedFee);

            var tx = builder.BuildTransaction(false);
            var usedCoins = tx.Inputs.Select(input => coins.First(o => o.Outpoint == input.PrevOut)).ToList();

            return BuildedTransaction.Create(tx, calculatedFee, amount, usedCoins);
        }
    }
}
