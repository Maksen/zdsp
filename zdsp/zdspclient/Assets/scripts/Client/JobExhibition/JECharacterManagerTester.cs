using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JECharacterManagerTester : MonoBehaviour {

    JECharacterManager manager = null;

    void init()
    {
        if (manager == null)
        {
            manager = gameObject.GetComponent<JECharacterManager>();
            manager.init();
        }
    }

    // Use this for initialization
    void Start () {
        init();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
