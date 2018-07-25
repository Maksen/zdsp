using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UI_FlickerFlare : MonoBehaviour {

    public CanvasGroup flareGroup;
    public float minIntensity = 0.7f;
    public float maxIntensity = 1f;
    public float speed = 15f;

    float random;

    // Use this for initialization
    void Start () {
        random = Random.Range(0.0f, 65535.0f);
    }
	
	// Update is called once per frame
	void Update () {
        float noise = Mathf.PerlinNoise(random, Time.time * speed);
        flareGroup.alpha = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
