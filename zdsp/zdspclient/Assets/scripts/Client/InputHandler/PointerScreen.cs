#if UNITY_EDITOR || UNITY_STANDALONE
#define ALLOW_LEFT_CLICK_ONLY
#endif

using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PointerScreen : MonoSingleton<PointerScreen>, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public delegate void PointerEventHandler(PointerEventData data);

    // Event sequence: OnPointerDown -> OnBeginDrag -> OnDrag -> OnPointerUp -> OnPointerClick -> OnEndDrag
    public static event PointerEventHandler OnPointerClickEvent;
    public static event PointerEventHandler OnPointerDownEvent;
    public static event PointerEventHandler OnPointerUpEvent;
    public static event PointerEventHandler OnDragEvent;
    public static event PointerEventHandler OnBeginDragEvent;
    public static event PointerEventHandler OnEndDragEvent;

    private List<PointerEventData> pointerList = new List<PointerEventData>();
    public int PointerCount { get { return pointerList.Count; } }

    public PointerEventData GetPointer(int index)
    {
        if (pointerList.Count > index)
            return pointerList[index];
        return null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
#endif
        if (OnPointerClickEvent != null)
            OnPointerClickEvent(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        ClearPointers();  // clear any lingering pointers in case pointerup not called
#endif
        pointerList.Add(eventData);
        if (OnPointerDownEvent != null)
            OnPointerDownEvent(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
#endif
        if (OnPointerUpEvent != null)
            OnPointerUpEvent(eventData);
        pointerList.Remove(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
#endif
        if (OnDragEvent != null)
            OnDragEvent(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
#endif
        if (OnBeginDragEvent != null)
            OnBeginDragEvent(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
#if ALLOW_LEFT_CLICK_ONLY
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
#endif
        if (OnEndDragEvent != null)
            OnEndDragEvent(eventData);
    }

    private void ClearPointers()
    {
        for (int i = 0; i < pointerList.Count; i++)
        {
            if (OnPointerUpEvent != null)
                OnPointerUpEvent(pointerList[i]);
        }
        pointerList.Clear();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            ClearPointers();  // clear pointer when application lose focus
        }
    }
}