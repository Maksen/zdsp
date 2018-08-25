using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
public class MapPointerEvent : MonoBehaviour, IPointerDownHandler
{
    RectTransform mRectTrans;
    public event UnityAction<Vector2> OnDown = delegate { };
    
    // Use this for initialization
    void Awake () {

        mRectTrans = GetComponent<RectTransform>();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localCursor = Vector2.zero;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTrans, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        OnDown(localCursor);
    }
}