using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panner : MonoBehaviour
{
    class Orientation
    {
        public Quaternion rot;
        public Vector3 pos;

        public Orientation(Transform t)
        {
            rot = t.rotation;
            pos = t.position;
        }

        public Orientation(Quaternion r, Vector3 p)
        {
            rot = r;
            pos = p;
        }

        public static Orientation Interpolate(Orientation a, Orientation b, float t)
        {
            return new Orientation(Quaternion.Lerp(a.rot, b.rot, t), a.pos * (1.0f - t) + b.pos * t);
        }

        public void SetTransform(Transform t)
        {
            t.rotation = rot;
            t.position = pos;            
        }
    };

    public List<Transform> Points;

    public float PanTime = 1.0f;

	// Use this for initialization
	void Start () {
        here = new Orientation(gameObject.GetComponent<Transform>());
        there = new Orientation(Points[0]);
    }

    Orientation here, there;
    float current_time = 0.0f;
	// Update is called once per frame
	void Update ()
    {
        current_time += Time.deltaTime;

        var t = current_time / PanTime;

        if (t >= 1.0f)
            t = 1.0f;

        var now = Orientation.Interpolate(here, there, t);

        now.SetTransform(gameObject.GetComponent<Transform>());
    }

    float GetT()
    {
        var t = current_time / PanTime;

        if (t >= 1.0f)
            t = 1.0f;

        t = Mathf.Sin(Mathf.PI / 2 * t);

        return t;
    }

    public void SetPoint(int index)
    {        
        var t = current_time / PanTime;

        if (t >= 1.0f)
            t = 1.0f;

        here = Orientation.Interpolate(here, there, t);
        there = new Orientation(Points[index % Points.Count]);

        current_time = 0.0f;
    }
}
