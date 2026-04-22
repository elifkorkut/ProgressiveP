using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProgressiveP.Core
{
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "Gameplay";

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    
}
}
