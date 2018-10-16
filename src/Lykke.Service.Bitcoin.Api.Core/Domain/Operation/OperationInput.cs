namespace Lykke.Service.Bitcoin.Api.Core.Domain.Operation
{
    public class OperationInput
    {
        public string Address { get; set; }

        public string AddressContext { get; set; }

        public long Amount { get; set; }
    }
}
