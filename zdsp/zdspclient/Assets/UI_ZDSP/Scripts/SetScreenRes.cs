using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenRes : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        int curWidth = Screen.width;
        int curHeight = Screen.height;
        Screen.SetResolution(1024, 1024 * curHeight / curWidth, true);
    }
	
	
}
