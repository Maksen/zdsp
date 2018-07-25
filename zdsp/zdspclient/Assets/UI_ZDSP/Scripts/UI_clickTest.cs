using UnityEngine;
using System.Collections;

public class UI_clickTest : MonoBehaviour {
    private int ClickCount = 1;

    void OnMouseUp()
    {
        Debug.LogError("Clicked <color=black>" + this.name + "</color> "+ ClickCount++ + "x");
    }
}
