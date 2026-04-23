using System;
using UnityEngine;
using UnityEngine.UI;
using ProgressiveP.Logic.Effects;
using  ProgressiveP.Core;
using  ProgressiveP.Logic;

namespace ProgressiveP.UI
{
   
public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private Transform startPanel;
    [SerializeField] private Button startButton;

    [SerializeField] private Transform blockerPanel;



    private void Awake()
{
        
            if (startPanel != null)
            {
                startPanel.gameObject.SetActive(false);
            }
            if (blockerPanel != null)
            {
                blockerPanel.gameObject.SetActive(true);
            }
    }



    private void OnEnable()
    {
       if( ServiceLocator.TryGet(out GameSessionManager sessionManager))
       {
              sessionManager.OnActiveSession+= HandleActiveSession;
              sessionManager.OnInactiveSession+= HandleInactiveSession;
              GameSessionManager.OnSessionLoaded += HandleActiveSessionLoaded;
       }
    }

    private void OnDisable()
    {
       if( ServiceLocator.TryGet(out GameSessionManager sessionManager))
       {
              sessionManager.OnActiveSession-= HandleActiveSession;
              sessionManager.OnInactiveSession-= HandleInactiveSession;
              GameSessionManager.OnSessionLoaded -= HandleActiveSessionLoaded;
       }
        
    }

       
    private void HandleActiveSession()
    {
        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(false);
        }
        if (blockerPanel != null)
        {
            blockerPanel.gameObject.SetActive(true);
        }
    }
 
  private void HandleActiveSessionLoaded()
    {
        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(false);
        }
        if (blockerPanel != null)
        {
            blockerPanel.gameObject.SetActive(false);
        }
    }

    private void HandleInactiveSession()
    {
        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(true);
            startButton.onClick.AddListener(OnStartClicked);
        }
        if (blockerPanel != null)
        {
            blockerPanel.gameObject.SetActive(true);
        }
    }

    public void OnStartClicked()
    { 
        SoundManager.instance.PlayClick();
       
        if (ServiceLocator.TryGet(out GameSessionManager sessionManager))       
        {
            sessionManager.onSessionRequested?.Invoke();
            startPanel.gameObject.SetActive(false);

        }
        
    }


}
}