using SoftMasking;
//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SoftMask))]
public class SoftMaskDelay : MonoBehaviour
{
    private bool mEnabled = true;
    private SoftMask component;

    void Awake()
    {
        component = GetComponent<SoftMask>();
    }

    void OnEnable()
    {
        mEnabled = false;
    }

    void OnGUI()
    {
        if (!mEnabled)
        {
            mEnabled = true;
            component.enabled = true;    
        }
    }
}

