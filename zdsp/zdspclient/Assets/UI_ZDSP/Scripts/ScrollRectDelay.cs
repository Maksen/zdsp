using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectDelay : MonoBehaviour
{
    public bool hasDefaultPos = true;
    public bool horizontalLeft = true;
    public bool vertialTop = true;

    private bool mEnabled = true;
    private ScrollRect component;

    void Awake()
    {
        component = GetComponent<ScrollRect>();
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
            if (hasDefaultPos)
            {
                if (component.horizontal)
                {
                    if (!horizontalLeft)
                        component.horizontalNormalizedPosition = 1;
                }
                if (component.vertical)
                {
                    if (!vertialTop)
                        component.verticalNormalizedPosition = 0;
                }
            }
        }
    }
}

