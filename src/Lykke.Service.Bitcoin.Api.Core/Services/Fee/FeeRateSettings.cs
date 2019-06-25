namespace Lykke.Service.Bitcoin.Api.Core.Services.Fee
{
    public class FeeRateSettings
    {
        public int DefaultFeeRatePerByte { get; set; }

        public int MinFeeRatePerByte { get; set; }
        public int MaxFeeRatePerByte { get; set; }
        public int FeeConfirmationTargetInBlocks { get; set; }
    }
}
