using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bitcoin.Api.Settings
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("/api/isalive")] public string ServiceUrl { get; set; }
    }
}
