using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveCameraTracker : MonoBehaviour
{
    static bool dirty = true;
    static int currentkey = 0;
    static Camera LastActiveCamera;
    public static Camera ActiveCamera
    {
        get
        {
            Camera ret = null;

            if (ActiveCameras.Count > 0)
            {
                if (dirty)
                {
                    foreach (var key in ActiveCameras)
                    {
                        currentkey = key.Key;
                    }

                    dirty = false;
                }
            }

            ActiveCameras.TryGetValue(currentkey, out ret);

            if (ret == null)
                ret = LastActiveCamera;

            return ret;
        }
    }

    static Dictionary<int, Camera> ActiveCameras = new Dictionary<int, Camera>();

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    static int key = 0;
    void OnEnable()
    {
        key = ++key;
        LastActiveCamera = this.GetComponent<Camera>();
        ActiveCameras.Add(key, LastActiveCamera);        

        dirty = true;
    }

    void OnDisable()
    {
        ActiveCameras.Remove(key);

        dirty = true;
    }
}
