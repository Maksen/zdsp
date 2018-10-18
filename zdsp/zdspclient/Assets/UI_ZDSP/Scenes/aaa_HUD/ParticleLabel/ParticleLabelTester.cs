using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLabelTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    float clock = 0.0f;
    public float period = 0.25f;
	void Update () {
        var plscript = GetComponent<ParticleLabel>();

        if (plscript != null && clock >= period)
        {
            clock = 0.0f;

            plscript.ToEmit = true;
        }

        clock += Time.deltaTime;
    }
}
