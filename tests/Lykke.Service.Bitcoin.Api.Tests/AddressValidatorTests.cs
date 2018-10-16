using Lykke.Service.Bitcoin.Api.Services.Address;
using NBitcoin;
using Xunit;

namespace Lykke.Service.Bitcoin.Api.Tests
{
    public class AddressValidatorTests
    {
        [Fact]
        public void CanDetectInvalidAddress()
        {
            var invalidAddress = "invalid";
            var addressValidator = new AddressValidator(Network.Main);

            Assert.False(addressValidator.IsValid(invalidAddress));
        }

        [Fact]
        public void CanPassValidMainetAddress()
        {
            var addresses = new[]
            {
                "19xM6HywehvSYfPvf3C8JVZPfE7zh1ziCD"
            };
            var addressValidator = new AddressValidator(Network.Main);

            foreach (var address in addresses) Assert.True(addressValidator.IsValid(address));
        }

        [Fact]
        public void CanPassValidTestnetAddress()
        {
            var addresses = new[]
            {
                "muLn6NV9aB9VLM7rJvh5i1wtUEXgDGNxW2"
            };
            var addressValidator = new AddressValidator(Network.TestNet);

            foreach (var address in addresses) Assert.True(addressValidator.IsValid(address));
        }
    }
}
