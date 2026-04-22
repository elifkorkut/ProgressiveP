using System.Threading.Tasks;

namespace ProgressiveP.Backend
{ public interface IPlayerService
    {
        Task<BackendResult<BackendData>> GetOrCreatePlayerAsync(string playerId);
        Task<BackendResult<BackendData>>GetPlayerAsync(string playerId);
        Task<BackendResult<bool>> SavePlayerAsync(BackendData player);
        Task<BackendResult<bool>> UpdateCoinsAsync(string playerId, double delta);

 }
}
