using UnityEngine;
using System.Collections;


// https://forum.unity.com/threads/how-to-calculate-horizontal-field-of-view.16114/
// Lock the cameras horizontal field of view so it will frame the same view in the horizontal regardless of aspect ratio.

[RequireComponent (typeof(Camera))]
public class HorizontalFOVLock : MonoBehaviour {
 
    public float fixedHorizontalFOV = 91.49285f;
    Camera cam;
    
    void OnEnable()
    {
        if (Application.isEditor)
        {
            //Debug.LogError("IS EDITOR!!!!");
            StartCoroutine(DoLoopCheck(0.5f));
        }
        else
        {
            //Debug.LogError("NOT EDITOR!!!!");
            setFoV();
        }
    }

    IEnumerator DoLoopCheck(float _time)
    {
        while (true)
        {
            //print("DoLoopCheck");
            setFoV();
            yield return new WaitForSeconds(_time);
        }
    }

    void setFoV()
    {
        if (GetComponent<Camera>().aspect < 1.85) // Do not set for FOV if it's 2:1 aspect (Samsung S8, iPhoneX..)
        {
            GetComponent<Camera>().fieldOfView = 2 * Mathf.Atan(Mathf.Tan(fixedHorizontalFOV * Mathf.Deg2Rad * 0.5f) / GetComponent<Camera>().aspect) * Mathf.Rad2Deg;
        }
        else
        {
            GetComponent<Camera>().fieldOfView = 60f;
        }
        //Debug.Log("Aspect: " + GetComponent<Camera>().aspect + " , FOV: " + GetComponent<Camera>().fieldOfView);
    }
}