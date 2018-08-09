using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatroomScrollView : MonoBehaviour
{
    [SerializeField]
    int paddingRows = 2;
    //[SerializeField]
    //int cellsPerRow = 4;
    [SerializeField]
    GameObject customRowPrefab;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    Transform contentTransform;
    [SerializeField]
    VerticalLayoutGroup verticalLayoutGrp;

    List<GameObject> contentRowList =  null;

    // Scroll rect properties
    int maxRows;
    float visibleScrollHeight;
    float rowHeight;
    int numRowsAvailable;
    int numRowsVisible;
    float contentYPivot;

    Vector2 lastScrollPos;

    // Vertical layout properties
    float topPadding;
    float cellHeight;

    int currentBottomIndex = 0;
    int currentLastRow = 1;

    public HUD_Chatroom HUDChatroom { private get; set; } // UI for this scrollview

    void OnDisable()
    {
        if (contentRowList != null)
        {
            int count = contentRowList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameObject rowData = contentRowList[i];
                if (rowData != null)
                {
                    Destroy(rowData);
                    rowData = null;
                }
            }
            contentRowList.Clear();
        }
    }

    void OnDestroy()
    {
        customRowPrefab = null;
        scrollRect = null;
        contentTransform = null;
        verticalLayoutGrp = null;

        contentRowList = null;

        HUDChatroom = null;
    }

    public void InitScrollView(HUD_Chatroom hudChatroom)
    {
        currentBottomIndex = 0;
        currentLastRow = 1;

        contentRowList = new List<GameObject>();

        visibleScrollHeight = gameObject.GetComponent<RectTransform>().rect.height;
        rowHeight = customRowPrefab.GetComponent<LayoutElement>().minHeight;
        contentYPivot = contentTransform.GetComponent<RectTransform>().pivot.y;

        HUDChatroom = hudChatroom;

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnUpdateScrollView);
    }

    public IEnumerator PopulateRows()
    {
        yield return null;
        numRowsAvailable = 0;

        int rowCount = HUD_Chatroom.chatLog[(byte)HUDChatroom.CurrentChannelTab].Count;
        InitRows(rowCount);
        currentBottomIndex = rowCount - 1;
        currentLastRow = rowCount;
        UpdateVisibleRows();

        yield return null;
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void SetRows(int numRowsVisible)
    {
        this.numRowsVisible = numRowsVisible;
        int paddedRows = numRowsVisible + paddingRows;
        numRowsAvailable = maxRows >= paddedRows ? paddedRows : maxRows;
    }

    public void InitRows(int rowCount)
    {
        maxRows = rowCount;

        topPadding = verticalLayoutGrp.padding.top;
        cellHeight = verticalLayoutGrp.spacing + rowHeight;

        SetRows((int)Math.Ceiling(visibleScrollHeight / cellHeight));

        // Create parent rows
        int contentRowCount = contentRowList.Count;
        int maxCount = rowCount > contentRowCount ? rowCount : contentRowCount;
        for (int i = 0; i < maxCount; ++i)
        {
            if (i == rowCount)
            {
                //contentRowList.RemoveRange(i, maxCount - rowCount);
                for (int j = maxCount-1; j >= rowCount; --j)
                {
                    Destroy(contentRowList[j]);
                    contentRowList.RemoveAt(j);
                }
                break;
            }
            else if (i == contentRowCount)
            {
                GameObject row = Instantiate(customRowPrefab);
                row.transform.SetParent(contentTransform, false);
                contentRowList.Add(row);
                ++contentRowCount;
            }
        }

        /*if (currentTopIndex + numRowsAvailable >= rowCount)
        {
            if (rowCount < numRowsVisible)
            {
                currentTopIndex = 0;
                currentFirstRow = 1;
            }
            else
            {
                currentTopIndex = rowCount - numRowsAvailable;
                if (currentTopIndex < 0)
                {
                    currentTopIndex = 0;
                }
            }
        }*/
    }

    public void UpdateVisibleRows()
    {
        // Update row child objects
        float totalHeight = 0;
        int j = currentBottomIndex;
        for (int i = 0; i < numRowsAvailable; ++i, --j)
        {
            ChatroomScrollRow chatroomScrollRow = contentRowList[j].GetComponent<ChatroomScrollRow>();
            chatroomScrollRow.Init(HUDChatroom, j);
            Canvas.ForceUpdateCanvases();
            totalHeight += chatroomScrollRow.GetRowHeight();
            if (totalHeight > visibleScrollHeight)
            {
                SetRows(currentBottomIndex - j);
                break;
            }
        }
        // Remove excess rows
        for (int i = j-paddingRows; i >= 0; --i)
            contentRowList[i].GetComponent<ChatroomScrollRow>().Clear();
    }

    void OnUpdateScrollView(Vector2 scrollPos)
    {
        float posY = contentTransform.localPosition.y;
        if (contentYPivot == 0)
            posY = -(posY + visibleScrollHeight);
        //Debug.LogFormat("posY: {0}", posY);

        posY -= topPadding;
        bool scrollUp = lastScrollPos.y > scrollPos.y;

        int newLast = Mathf.Clamp(BottomRowSeen(posY), numRowsAvailable, maxRows);
        //int newFirst = Mathf.Clamp(newLast-numRowsVisible+1, 1, maxRows-numRowsVisible+1);
        int newFirst = newLast - numRowsVisible + 1;
        //Debug.LogFormat("newFirst: {0}, newLast: {1}", newFirst, newLast);

        if (currentLastRow != newLast)
        {
            if (scrollUp && newFirst > 1 && currentLastRow < maxRows && currentBottomIndex < contentRowList.Count-1)
            {
                int diff = newLast - currentLastRow;
                for (int i = 0; i < diff; ++i)
                {
                    int newIndex = currentBottomIndex + 1;

                    contentRowList[currentBottomIndex-numRowsAvailable+1].GetComponent<ChatroomScrollRow>().Clear();
                    contentRowList[newIndex].GetComponent<ChatroomScrollRow>().Init(HUDChatroom, newIndex);

                    ++currentBottomIndex;
                }
            }
            else if (!scrollUp && newFirst > 1 && (currentBottomIndex-numRowsAvailable+1) > 0)
            {
                int diff = currentLastRow - newLast;
                for (int i = 0; i < diff; ++i)
                {
                    int newIndex = currentBottomIndex - numRowsAvailable;

                    contentRowList[currentBottomIndex].GetComponent<ChatroomScrollRow>().Clear();
                    contentRowList[newIndex].GetComponent<ChatroomScrollRow>().Init(HUDChatroom, newIndex);

                    --currentBottomIndex;
                }
            }
            currentLastRow = newLast;
        }
        lastScrollPos = scrollPos;
    }

    int BottomRowSeen(float posY)
    {
        int hiddenGrids = contentRowList.Count;
        float currHeight = 0;
        for (int i = hiddenGrids-1; i >= 0; --i)
        {
            currHeight += contentRowList[i].GetComponent<RectTransform>().rect.height;
            if (posY > currHeight)
                --hiddenGrids;
            else
                break;
        }
        return hiddenGrids;
    }
}
