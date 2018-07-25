using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UI_Parallax : MonoBehaviour {

    public Transform ScrollRectContent;
    public float Speed = 1;
    public bool ReverseDirection;

    public void Parallax()
    {
        float xPosition = Mathf.Lerp(ScrollRectContent.localPosition.x, 0f,0f) * Speed;

        if (!ReverseDirection)
        {
            gameObject.transform.localPosition = new Vector2(xPosition, 0f);
        }
        else
        {
            gameObject.transform.localPosition = new Vector2(xPosition *-1, 0f);
        }
        
        //Debug.Log("Moved" + xPosition);
    }
}
