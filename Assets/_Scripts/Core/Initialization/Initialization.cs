using System.Threading.Tasks;
using ProgressiveP.Backend;
using UnityEngine;

namespace ProgressiveP.Core
{
   public class Initialization : MonoBehaviour
    {
        [SerializeField] private Authenticator authenticator;
        [SerializeField] private SceneLoader sceneLoader;

        private async void Start()
        {
            if (authenticator == null)
            {
                Debug.LogError("[Initialization] Authenticator reference is not assigned.");
                return;
            }
            
            await authenticator.InitializeAndLoginAsync(HandleAuthenticated);
        }

        private async void HandleAuthenticated(string playerId)
        {
            await MockServices.Instance.Initialize();
            await MockServices.Instance.AuthenticatePlayerAsync(playerId,HandleError, HandleSuccess);
         }

         private void HandleError(string errorMessage)
         {
             Debug.LogError($"[Initialization] Authentication error: {errorMessage}");
         }

         private void HandleSuccess( BackendData data)
         {
             Debug.Log("[Initialization] Player authenticated successfully.");
             DataReader.LoadPlayerData(data.value);
             LoadScene();
         }

        public void LoadScene()
        {
            if (sceneLoader == null)
            {
                Debug.LogError("[Initialization] SceneLoader reference is not assigned.");
                return;
            }
            sceneLoader.LoadScene();
        }

       
    }
}
