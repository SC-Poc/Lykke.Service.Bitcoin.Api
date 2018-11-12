using System.Threading.Tasks;

namespace Lykke.Service.Bitcoin.Api.Core.Domain.Wallet
{
    public interface ILastProcessedBlockRepository
    {
        Task<int?> GetLastProcessedBlock();

        Task SetLastProcessedBlock(int height);
    }
}
