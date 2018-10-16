namespace Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs
{
    public interface IOutput
    {
        string TransactionHash { get; }

        int N { get; }
    }
}
