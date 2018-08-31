namespace Lykke.Service.Bitcoin.Api.Core.Domain.Fee
{
    public interface IFeeRate
    {
        int FeePerByte { get; }
    }
}
