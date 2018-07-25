using UnityEngine;
using System.Collections;

//Sets the Child GameObject to be Active when not running from Editor.

public class UI_ActivateInApp : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (!Application.isEditor)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        
    }
}
