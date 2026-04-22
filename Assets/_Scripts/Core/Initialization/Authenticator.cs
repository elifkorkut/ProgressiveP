using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace ProgressiveP.Core
{
 
    public class Authenticator : MonoBehaviour
    {   

     public static string playerId;
    public async Task<string> InitializeAndLoginAsync( System.Action<string> onAuthenticated)
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized.");
            SetupEvents();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            onAuthenticated?.Invoke(AuthenticationService.Instance.PlayerId);
            playerId = AuthenticationService.Instance.PlayerId;
            return AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex)
        {
             Debug.LogException(ex);
            return null;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Event: Player signed in.");
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Event: Player signed out.");
        };

        AuthenticationService.Instance.Expired += () => {
            Debug.Log("Event: Player session expired.");
        };
    }
}
        
}
