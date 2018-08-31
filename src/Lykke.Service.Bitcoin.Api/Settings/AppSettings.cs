using Lykke.Sdk.Settings;
using Lykke.Service.Bitcoin.Api.Settings.ServiceSettings;

namespace Lykke.Service.Bitcoin.Api.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public BitcoinApiSettings Bitcoin { get; set; }

        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
    }
}
