using System;
using Autofac;
using Lykke.Service.Assets.Client;
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
            builder.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)));
        }
    }
}
