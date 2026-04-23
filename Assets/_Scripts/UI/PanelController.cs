using UnityEngine;
using UnityEngine.UI;
using ProgressiveP.Logic.Effects;
public class PanelController : MonoBehaviour
{
  [SerializeField] private GameObject panelToShow;
  [SerializeField] private Button buttonToClick;
  [SerializeField] private PanelCloser panelCloser;

  public event System.Action OnPanelShown;
  public event System.Action OnPanelHidden;

    void Awake()
    {
         panelToShow.SetActive(false);
       
    }
    void OnEnable()
    {
        buttonToClick.onClick.AddListener(ShowPanelMethod);

          if (panelCloser != null)
         {
             panelCloser.OnClick += ShowPanelMethod;
         }
     }

    void OnDisable()
    {
        if (panelCloser != null)
        {
            panelCloser.OnClick -= ShowPanelMethod;
        }
        buttonToClick.onClick.RemoveListener(ShowPanelMethod);
    }
   
    public void ShowPanelMethod()
    {  
        SoundManager.instance.PlayClick();
        panelToShow.SetActive(!panelToShow.activeSelf);
        if (panelToShow.activeSelf)
        {
            OnPanelShown?.Invoke();
        }
        else
        {
            OnPanelHidden?.Invoke();
        }
    }


}
