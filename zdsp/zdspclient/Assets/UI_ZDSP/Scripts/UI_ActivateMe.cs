using UnityEngine;
using System.Collections;

//Set a Delay before Activating a GameObject.
//Useful when there's a loop animation, but needs a Delay before staring the loop animation.

public class UI_ActivateMe : MonoBehaviour {

	public float delay;
	private float cachedDelay;
	public GameObject[] GOList;
    private bool isAllActive = false;
	
	// Use this for initialization
	void Start () {
		cachedDelay = delay;
	}
	
	// Update is called once per frame
	void Update () {
		if (delay > 0)
		{
			delay -= Time.deltaTime;
			//Debug.Log ("Delay: " + delay);
		}
		
		if (delay <= 0.0f && !isAllActive)
		{
			if(GOList.Length > 0)
			{
				foreach(GameObject go in GOList)
				{
					if(go.activeSelf == true)
					{
						continue;
					}

					go.SetActive(true);
					//gameObject.SetActive(true);
					//Debug.Log(go + " is active!");
				}
            }

            isAllActive = true;
        }
	}
}
