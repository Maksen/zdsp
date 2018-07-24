using UnityEngine;
using System.Collections;

public class SceneEffect : MonoBehaviour
{
    public GameObject mainCamera;
    public float heightOffset = 0;
    public float forwardOffset = 0;

    public void InitCamera(GameObject cam)
    {
        mainCamera = cam;
    }

	void LateUpdate ()
    {
	    if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position;
            transform.Translate(0, heightOffset, 0);
            Vector3 dir = new Vector3(0, 0, forwardOffset);
            dir = mainCamera.transform.TransformDirection(dir);
            dir.y = 0;
            dir.Normalize();
            transform.Translate(dir * forwardOffset);
        }
	}
}
