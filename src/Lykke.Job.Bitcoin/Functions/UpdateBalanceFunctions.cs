using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Services.Operations;

namespace Lykke.Job.Bitcoin.Functions
{
    public class UpdateBalanceFunctions
    {
        private readonly OperationsConfirmationsSettings _confirmationsSettings;
        private readonly ILog _log;
        private readonly IObservableWalletRepository _observableWalletRepository;
        private readonly IWalletBalanceService _walletBalanceService;

        public UpdateBalanceFunctions(IObservableWalletRepository observableWalletRepository,
            OperationsConfirmationsSettings confirmationsSettings,
            IWalletBalanceService walletBalanceService, ILogFactory logFactory)
        {
            _observableWalletRepository = observableWalletRepository;
            _confirmationsSettings = confirmationsSettings;
            _walletBalanceService = walletBalanceService;
            _log = logFactory.CreateLog(this);
        }

        [TimerTrigger("00:05:00")]
        public async Task UpdateBalances()
        {
            string continuation = null;
            do
            {
                IEnumerable<IObservableWallet> wallets;
                (wallets, continuation) = await _observableWalletRepository.GetAllAsync(1000, continuation);
                foreach (var observableWallet in wallets)
                    try
                    {
                        await _walletBalanceService.UpdateBalanceAsync(observableWallet,
                            _confirmationsSettings.MinConfirmationsToDetectOperation);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, null, observableWallet.ToJson());
                    }
            } while (continuation != null);
        }
    }
}
