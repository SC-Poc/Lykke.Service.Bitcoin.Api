using System;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.Assets.Client;
using Lykke.Service.Bitcoin.Api.Lifetime;
using Lykke.Service.Bitcoin.Api.Services;
using Lykke.Service.Bitcoin.Api.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Bitcoin.Api.Modules
{
    public class BitcoinApiModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public BitcoinApiModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<FakeAssetClient>()
                .As<IAssetsServiceWithCache>()
                .SingleInstance();
        }
    }
}
