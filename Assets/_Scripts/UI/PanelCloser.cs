using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PanelCloser : MonoBehaviour, IPointerClickHandler
{
   
   public event Action OnClick;
 

   public void OnPointerClick(PointerEventData eventData)
   {
      CallMethod();
   }

   public void CallMethod()
   {
      OnClick?.Invoke();
     
   }
}
