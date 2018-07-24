using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class PointerEventController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler,IPointerDownHandler
{
    public UnityEvent OnPointerEnterEvent;
    public UnityEvent OnPointerExitEvent;
    public UnityEvent OnPointerDownEvent;
    public UnityEvent OnPointerUpEvent;


    public void OnPointerEnter(PointerEventData eventData)
    {
      //  Debug.Log("enter");
        OnPointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("up");
        OnPointerUpEvent.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownEvent.Invoke();
    }
    public void abc()
    {

    }

}
