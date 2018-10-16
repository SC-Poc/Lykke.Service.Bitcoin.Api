using Autofac;
using AzureStorage.Blob;
using Lykke.Common.Log;
using Lykke.Job.Bitcoin.Settings;
using Lykke.Service.Bitcoin.Api.AzureRepositories.Asset;
using Lykke.Service.Bitcoin.Api.AzureRepositories.Fee;
using Lykke.Service.Bitcoin.Api.AzureRepositories.Operations;
using Lykke.Service.Bitcoin.Api.AzureRepositories.SpentOutputs;
using Lykke.Service.Bitcoin.Api.AzureRepositories.Transactions;
using Lykke.Service.Bitcoin.Api.AzureRepositories.Wallet;
using Lykke.Service.Bitcoin.Api.Core.Domain.Asset;
using Lykke.Service.Bitcoin.Api.Core.Domain.Fee;
using Lykke.Service.Bitcoin.Api.Core.Domain.ObservableOperation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Operation;
using Lykke.Service.Bitcoin.Api.Core.Domain.Outputs;
using Lykke.Service.Bitcoin.Api.Core.Domain.Transactions;
using Lykke.Service.Bitcoin.Api.Core.Domain.Wallet;
using Lykke.SettingsReader;

namespace Lykke.Job.Bitcoin.Modules
{
    public class RepositoryModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public RepositoryModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterRepo(builder);
            RegisterBlob(builder);
        }

        private void RegisterRepo(ContainerBuilder builder)
        {
            builder.Register(x => new AssetRepository())
                .As<IAssetRepository>()
                .SingleInstance();

            var connectionString = _settings.Nested(p => p.Bitcoin.Db.DataConnString);

            builder.Register(x => OperationMetaRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IOperationMetaRepository>()
                .SingleInstance();

            builder.Register(x => OperationEventRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IOperationEventRepository>()
                .SingleInstance();

            builder.Register(x => UnconfirmedTransactionRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IUnconfirmedTransactionRepository>()
                .SingleInstance();


            builder.Register(x => ObservableOperationRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IObservableOperationRepository>()
                .SingleInstance();

            builder.Register(x => ObservableWalletRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IObservableWalletRepository>()
                .SingleInstance();

            builder.Register(x => WalletBalanceRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IWalletBalanceRepository>()
                .SingleInstance();

            builder.Register(x => SpentOutputRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<ISpentOutputRepository>()
                .SingleInstance();

            builder.Register(x => FeeRateRepository.Create(connectionString, x.Resolve<ILogFactory>()))
                .As<IFeeRateRepository>()
                .SingleInstance();
        }

        private void RegisterBlob(ContainerBuilder builder)
        {
            builder.RegisterInstance(
                    new TransactionBlobStorage(
                        AzureBlobStorage.Create(_settings.Nested(p => p.Bitcoin.Db.DataConnString))))
                .As<ITransactionBlobStorage>();
        }
    }
}
