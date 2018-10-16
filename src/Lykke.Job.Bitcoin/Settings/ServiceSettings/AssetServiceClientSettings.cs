using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bitcoin.Settings.ServiceSettings
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("/api/isalive")] public string ServiceUrl { get; set; }
    }
}
