namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public class ObservableWallet : IObservableWallet
    {
        public string Address { get; set; }

        public static ObservableWallet Create(string address)
        {
            return new ObservableWallet
            {
                Address = address
            };
        }
    }
}
