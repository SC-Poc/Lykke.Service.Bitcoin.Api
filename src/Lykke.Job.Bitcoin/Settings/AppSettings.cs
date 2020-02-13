using Lykke.Job.Bitcoin.Settings.ServiceSettings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bitcoin.Settings
{
    public class AppSettings
    {
        public BitcoinJobSettings Bitcoin { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional] public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
