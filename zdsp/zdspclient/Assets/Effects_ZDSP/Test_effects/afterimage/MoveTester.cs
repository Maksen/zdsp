using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    public float speed = 1.0f;
	void Update () {
        transform.localPosition = transform.localPosition + new Vector3(0.0f, 0.0f, speed * Time.deltaTime);
	}
}
