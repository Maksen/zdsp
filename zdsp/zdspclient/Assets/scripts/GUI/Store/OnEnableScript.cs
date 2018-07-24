using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableScript : MonoBehaviour {

    public delegate void OnEnabled(GameObject obj);
    public OnEnabled onEnabled = null;
    private void OnEnable()
    {
        if (onEnabled == null) return;

        onEnabled(gameObject);
    }

    public delegate void OnDisabled(GameObject obj);
    public OnDisabled onDisabled = null;
    private void OnDisable()
    {
        if (onDisabled == null) return;

        onDisabled(gameObject);
    }
}
