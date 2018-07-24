using UnityEngine;
using System.Collections;

public class EffectObjectController : MonoBehaviour {
    private Animation Animation;
    private bool hide = false;

    // Use this for initialization
    void Start () {
        Animation = gameObject.GetComponent<Animation>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Animation.isPlaying)
        {
            if (!hide)
            {
                foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                    renderer.enabled = true;
                hide = true;
            }
        }
        else
        {
            if (hide)
            {
                foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                    renderer.enabled = false;
                hide = false;
            }
        }
    }

    public void Replay()
    {
        Animation.Play();
    }
}
