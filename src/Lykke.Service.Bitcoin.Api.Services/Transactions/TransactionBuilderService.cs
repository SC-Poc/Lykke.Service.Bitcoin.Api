﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.Address;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Lykke.Service.Bitcoin.Api.Core.Services.Fee;
using Lykke.Service.Bitcoin.Api.Core.Services.Operation;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using Lykke.Service.Bitcoin.Api.Core.Services.Transactions;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.Transactions
{
    public class TransactionBuilderService : ITransactionBuilderService
    {
        private readonly IFeeService _feeService;
        private readonly ITransactionOutputsService _transactionOutputsService;
        private readonly IAddressValidator _addressValidator;

        public TransactionBuilderService(ITransactionOutputsService transactionOutputsService,
            IAddressValidator addressValidator,
            IFeeService feeService)
        {
            _transactionOutputsService = transactionOutputsService;
            _addressValidator = addressValidator;
            _feeService = feeService;
        }

        private async Task<IList<Coin>> GetUnspentCoins(OperationBitcoinInput input)
        {
            var coins = (await _transactionOutputsService.GetUnspentOutputsAsync(input.Address.ToString())).ToList();
            if (input.Redeem != null)
                coins = coins.Select(o => o.ToScriptCoin(input.Redeem)).Cast<Coin>().ToList();
            return coins;
        }


        public async Task<IBuiltTransaction> GetManyOutputsTransferTransactionAsync(OperationInput fromAddress,
            IList<OperationOutput> toAddresses)
        {
            var input = fromAddress.ToBitcoinInput(_addressValidator);
            var outputs = toAddresses.Select(o => o.ToBitcoinOutput(_addressValidator)).ToList();
            var builder = new TransactionBuilder();

            foreach (var operationBitcoinOutput in outputs)
                builder.Send(operationBitcoinOutput.Address, operationBitcoinOutput.Amount);

            var coins = await GetUnspentCoins(input);

            builder.AddCoins(coins).SetChange(input.Address);

            var calculatedFee = await _feeService.CalcFeeForTransactionAsync(builder);

            var requiredBalance = outputs.Sum(o => o.Amount) + calculatedFee;
            var totalBalance = coins.Sum(o => o.Amount);

            if (totalBalance < requiredBalance)
                throw new BusinessException($"The sum of total applicable outputs is less than the required : {requiredBalance} satoshis.", ErrorCode.NotEnoughFundsAvailable);

            builder.SendFees(calculatedFee);

            var tx = builder.BuildTransaction(false);

            return BuiltTransaction.Create(tx, calculatedFee, builder.FindSpentCoins(tx).Cast<Coin>());
        }



        public async Task<IBuiltTransaction> GetManyInputsTransferTransactionAsync(IList<OperationInput> fromAddresses, OperationOutput toAddress)
        {
            var inputs = fromAddresses.Select(o => o.ToBitcoinInput(_addressValidator)).ToList();
            var output = toAddress.ToBitcoinOutput(_addressValidator);

            var builder = new TransactionBuilder();

            var totalBalance = Money.Zero;
            var sendAmount = inputs.Sum(o => o.Amount);

            foreach (var operationBitcoinInput in inputs)
            {
                var coins = await GetUnspentCoins(operationBitcoinInput);
                builder.AddCoins(coins);

                var addressBalance = coins.Sum(o => o.Amount);

                if (addressBalance < operationBitcoinInput.Amount)
                    throw new BusinessException($"The sum of total applicable outputs is less than the required : {operationBitcoinInput.Amount.Satoshi} satoshis.",
                        ErrorCode.NotEnoughFundsAvailable);

                // send change to source address
                var change = addressBalance - operationBitcoinInput.Amount;
                if (change < new TxOut(Money.Zero, operationBitcoinInput.Address).GetDustThreshold(builder.StandardTransactionPolicy.MinRelayTxFee))
                    sendAmount += change;
                else
                    builder.Send(operationBitcoinInput.Address, change);
                totalBalance += addressBalance;
            }
            builder.Send(output.Address, sendAmount);

            var calculatedFee = await _feeService.CalcFeeForTransactionAsync(builder);

            var requiredBalance = sendAmount;

            if (totalBalance < requiredBalance)
                throw new BusinessException(
                    $"The sum of total applicable outputs is less than the required : {requiredBalance} satoshis.",
                    ErrorCode.NotEnoughFundsAvailable);

            if (calculatedFee > sendAmount)
                throw new BusinessException(
                    $"The sum of total applicable outputs is less than the required fee:{calculatedFee} satoshis.",
                    ErrorCode.BalanceIsLessThanFee);
            if (calculatedFee > 0)
            {
                builder.SubtractFees();
                builder.SendFees(calculatedFee);
            }

            var tx = builder.BuildTransaction(false);

            return BuiltTransaction.Create(tx, calculatedFee, builder.FindSpentCoins(tx).Cast<Coin>());
        }


        public async Task<IBuiltTransaction> GetTransferTransactionAsync(OperationInput fromAddress, OperationOutput toAddress, bool includeFee)
        {
            var input = fromAddress.ToBitcoinInput(_addressValidator);
            var output = toAddress.ToBitcoinOutput(_addressValidator);

            var builder = new TransactionBuilder();

            var coins = await GetUnspentCoins(input);
            builder.AddCoins(coins);

            var addressBalance = coins.Sum(o => o.Amount);

            if (addressBalance < input.Amount)
                throw new BusinessException($"The sum of total applicable outputs is less than the required : {input.Amount.Satoshi} satoshis.",
                    ErrorCode.NotEnoughFundsAvailable);

            var sendAmount = input.Amount;
            var sentFees = Money.Zero;

            var change = addressBalance - input.Amount;
            if (change < new TxOut(Money.Zero, input.Address).GetDustThreshold(builder.StandardTransactionPolicy.MinRelayTxFee).Satoshi)
            {
                if (includeFee)
                    sendAmount += change;
                else
                {
                    builder.SendFees(change);
                    sentFees = change;
                }
            }

            builder.Send(output.Address, sendAmount).SetChange(input.Address);

            var calculatedFee = Money.Max(Money.Zero, await _feeService.CalcFeeForTransactionAsync(builder) - sentFees);

            var requiredBalance = input.Amount + (includeFee ? Money.Zero : calculatedFee);

            if (addressBalance < requiredBalance)
                throw new BusinessException(
                    $"The sum of total applicable outputs is less than the required : {requiredBalance} satoshis.",
                    ErrorCode.NotEnoughFundsAvailable);

            if (includeFee)
            {
                if (calculatedFee > input.Amount)
                    throw new BusinessException(
                        $"The sum of total applicable outputs is less than the required fee:{calculatedFee} satoshis.",
                        ErrorCode.BalanceIsLessThanFee);
                builder.SubtractFees();
            }

            if (calculatedFee > 0)
                builder.SendFees(calculatedFee);
            var tx = builder.BuildTransaction(false);

            return BuiltTransaction.Create(tx, calculatedFee + sentFees, builder.FindSpentCoins(tx).Cast<Coin>());
        }
    }
}
