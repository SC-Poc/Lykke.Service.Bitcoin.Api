using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bitcoin.Settings
{
    public class MonitoringServiceClientSettings
    {
        /// <summary>
        ///     Gets or sets the monitoring service URL.
        /// </summary>
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
