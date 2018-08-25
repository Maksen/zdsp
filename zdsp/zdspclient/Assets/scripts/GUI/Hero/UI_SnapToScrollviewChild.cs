using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_SnapToScrollviewChild : MonoBehaviour
{
    [SerializeField] GameObject dataPrefab;

    private ScrollRect scrollRect;
    private RectTransform rectTransform;
    private RectTransform contentRectTransform;
    private VerticalLayoutGroup verticalLayout;

    private float cellHeight;
    private int numRowsTotallyVisible;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        rectTransform = GetComponent<RectTransform>();
        contentRectTransform = scrollRect.content;
        verticalLayout = contentRectTransform.GetComponent<VerticalLayoutGroup>();

        float visibleHeight = rectTransform.rect.height;
        float dataHeight = dataPrefab.GetComponent<RectTransform>().rect.height;
        cellHeight = verticalLayout.spacing + dataHeight;

        numRowsTotallyVisible = (int)Math.Floor(visibleHeight / cellHeight);
    }

    public void SnapToChild(int childIndex)
    {
        int childCount = contentRectTransform.childCount;
        if (childIndex >= 0 && childIndex < childCount - 1)
        {
            if (childCount <= numRowsTotallyVisible)
                scrollRect.verticalNormalizedPosition = 1f;
            else if (childCount - childIndex <= numRowsTotallyVisible)  // child to snap is within last few visible rows
                scrollRect.verticalNormalizedPosition = 0f;
            else
            {
                float y = cellHeight * childIndex;
                contentRectTransform.localPosition = new Vector2(contentRectTransform.localPosition.x, y);
            }
        }
    }
}