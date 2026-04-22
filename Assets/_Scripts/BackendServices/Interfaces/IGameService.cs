using System.Threading.Tasks;

namespace ProgressiveP.Backend
{
    public interface IGameService
    { 
         Task<BackendResult<BackendData>> GetGameConfigAsync(string gameId, System.Action<string> onError, System.Action<BackendData> onSuccess);
         Task<BackendResult<BackendData>> GetGameConfigGlobalAsync(string gameId, System.Action<string> onError, System.Action<BackendData> onSuccess);

         
    }
    
}
