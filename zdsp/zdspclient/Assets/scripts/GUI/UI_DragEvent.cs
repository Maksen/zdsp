using UnityEngine;
using UnityEngine.EventSystems; // Required when using event data
using System;

[RequireComponent(typeof(UnityEngine.UI.Image))]//requires this to block raycast
public class UI_DragEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler // required interface when using the OnDrag method.
{
    [SerializeField]
    float offsetLimit = 0.1f;
    
    public Action<Vector2> onDragging;
    public Action onClicked;

    bool isDragging;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.delta.magnitude > offsetLimit)
            isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDragging != null)
            onDragging(eventData.delta);

        //onDragging?.Invoke(eventData.scrollDelta);
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
        {
            if (onClicked != null)
                onClicked();
        }
    }

    void OnDestroy()
    {
        onDragging = null;
    }
}