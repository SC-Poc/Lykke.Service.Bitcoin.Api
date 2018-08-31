using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Bitcoin.Api.Core.Domain;
using Lykke.Service.Bitcoin.Api.Core.Services.BlockChainReaders;
using Lykke.Service.Bitcoin.Api.Core.Services.TransactionOutputs;
using NBitcoin;

namespace Lykke.Service.Bitcoin.Api.Services.TransactionOutputs
{
    public class TransactionOutputsService : ITransactionOutputsService
    {
        private readonly IBlockChainProvider _blockChainProvider;
        private readonly ISpentOutputRepository _spentOutputRepository;

        public TransactionOutputsService(IBlockChainProvider blockChainProvider,
            ISpentOutputRepository spentOutputRepository)
        {
            _blockChainProvider = blockChainProvider;
            _spentOutputRepository = spentOutputRepository;
        }

        public async Task<IEnumerable<Coin>> GetUnspentOutputsAsync(string address, int confirmationsCount = 0)
        {
            return await Filter(await _blockChainProvider.GetUnspentOutputsAsync(address, confirmationsCount));
        }

        private async Task<IEnumerable<Coin>> Filter(IList<Coin> coins)
        {
            var spentOutputs = new HashSet<OutPoint>(
                (await _spentOutputRepository.GetSpentOutputsAsync(coins.Select(o => new Output(o.Outpoint))))
                .Select(o => new OutPoint(uint256.Parse(o.TransactionHash), o.N)));
            return coins.Where(c => !spentOutputs.Contains(c.Outpoint));
        }
    }
}
