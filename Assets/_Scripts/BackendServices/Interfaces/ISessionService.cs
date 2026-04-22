using System;
using System.Threading.Tasks;

namespace ProgressiveP.Backend
{
    public interface ISessionService
    {
        Task<BackendResult<BackendData>> CreateSessionAsync(
            string playerId, string gameId,
            Action<string>      onError,
            Action<BackendData> onSuccess);
    }
}
    

