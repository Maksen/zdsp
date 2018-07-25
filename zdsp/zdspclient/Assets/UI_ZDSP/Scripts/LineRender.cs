using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 


[RequireComponent(typeof(LineRenderer))]
public class LineRender : MonoBehaviour {

    [SerializeField]
    Transform attachedGO0, attachedGO1;
    
    LineRenderer line_comp;

    [SerializeField]
    float radius0 = 0.5f, radius1 = 0.5f;

    void Awake()
    {
        line_comp = GetComponent<LineRenderer>();
    }

    // Use this for initialization
    void Start ()
    {
        line_comp.useWorldSpace = true;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        var direction = attachedGO1.transform.position - attachedGO0.transform.position;

        //prevent divide by zero error
        if (direction.x == 0)
            direction.x = 0.001f;

        //radius of ellipse

        var angle0 = Mathf.Atan(direction.y / direction.x);
        float a = attachedGO0.transform.localScale.x * radius0;
        float b = attachedGO0.transform.localScale.y * radius0;
        float radius;

        radius = (a * b) / Mathf.Sqrt(a * a * (Mathf.Sin(angle0)) * (Mathf.Sin(angle0)) + b * b * (Mathf.Cos(angle0)) * (Mathf.Cos(angle0)));
        line_comp.SetPosition(0, attachedGO0.transform.position + direction.normalized * radius);

        var angle1 = -1 * Mathf.Atan(direction.y / direction.x);//another way
        a = attachedGO1.transform.localScale.x * radius1;
        b = attachedGO1.transform.localScale.y * radius1;

        radius = (a * b) / Mathf.Sqrt(a * a * (Mathf.Sin(angle1)) * (Mathf.Sin(angle1)) + b * b * (Mathf.Cos(angle1)) * (Mathf.Cos(angle1)));
        line_comp.SetPosition(1, attachedGO1.transform.position - direction.normalized * radius);
    }



    public void DrawLine()
    {
        line_comp = GetComponent<LineRenderer>();
        line_comp.useWorldSpace = true;
        LateUpdate();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LineRender))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LineRender myScript = (LineRender)target;
        if (GUILayout.Button("Draw Line"))
        {
            myScript.DrawLine();
        }
    }
}
#endif