using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_SwitchCanvasScaler : MonoBehaviour {

    public void SetHUDCanvas()
    {
        //Match Width Or Height
        //1024 x 768
        CanvasScaler c = GetComponent<CanvasScaler>();
        c.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        c.referenceResolution = new Vector2(1024, 768);
    }

    public void SetUICanvas()
    {
        //Expand
        //1024 x 576
        CanvasScaler c = GetComponent<CanvasScaler>();
        c.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        c.referenceResolution = new Vector2(1024, 576);
    }
}
