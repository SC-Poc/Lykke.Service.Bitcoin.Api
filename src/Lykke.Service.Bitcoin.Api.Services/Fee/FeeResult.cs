using Newtonsoft.Json;

namespace Lykke.Service.Bitcoin.Api.Services.Fee
{
    public class FeeResult
    {
        [JsonProperty("fastestFee")] public int FastestFee { get; set; }

        [JsonProperty("halfHourFee")] public int HalfHourFee { get; set; }

        [JsonProperty("hourFee")] public int HourFee { get; set; }
    }
}
