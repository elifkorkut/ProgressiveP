using System.Threading.Tasks;

namespace ProgressiveP.Backend
{ public interface IPlayerService
    {
        Task<BackendResult<BackendData>> GetOrCreatePlayerAsync(string playerId);
        Task<BackendResult<BackendData>>GetPlayerAsync(string playerId);
     

 }
}
