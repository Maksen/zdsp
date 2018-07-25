using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CubeRotate : MonoBehaviour {
    public float turnSpeed = 50f;
    public bool Right;

    // Update is called once per frame
    void Update () {
        if (!Right)
        {
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
        } else {
            transform.Rotate(Vector3.right * turnSpeed * Time.deltaTime);
        }
        
    }
}
