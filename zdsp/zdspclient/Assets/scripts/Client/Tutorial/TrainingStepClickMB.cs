using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TrainingStepClickMB : MonoBehaviour, IPointerDownHandler {

	// Use this for initialization
	public void OnPointerDown(PointerEventData edata)
    {
        gameObject.SetActive(false);
    }
}
