using System;
using Autofac;
using Lykke.Job.Bitcoin.Settings;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.Assets.Client;
using Lykke.Service.Bitcoin.Api.Core.Services;
using Lykke.Service.Bitcoin.Api.Services.Health;
using Lykke.SettingsReader;

namespace Lykke.Job.Bitcoin.Modules
{
    public class BitcoinJobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public BitcoinJobModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)));
            builder.AddTriggers();
        }
    }
}
