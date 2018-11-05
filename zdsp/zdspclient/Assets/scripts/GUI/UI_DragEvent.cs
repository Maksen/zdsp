using System;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using event data

[RequireComponent(typeof(UnityEngine.UI.Image))]//requires this to block raycast
public class UI_DragEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler // required interface when using the OnDrag method.
{
    [SerializeField]
    float offsetLimit = 0.1f;

    public Action onBeginDrag;
    public Action<Vector2> onDragging;
    public Action onClicked;

    private bool isDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.delta.magnitude > offsetLimit)
        {
            isDragging = true;
            if (onBeginDrag != null)
                onBeginDrag();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDragging != null)
            onDragging(eventData.delta);
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
}