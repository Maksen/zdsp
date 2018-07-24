using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScrollView : MonoBehaviour
{
    [SerializeField]
    int paddingRows = 2;
    [SerializeField]
    int cellsPerRow = 4;
    [SerializeField]
    GameObject customRowPrefab;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    Transform contentTransform;
    [SerializeField]
    VerticalLayoutGroup verticalLayoutGrp;

    List<GameObject> contentRowList;

    // Scroll rect properties
    int maxRows;
    float visibleScrollHeight;
    float rowHeight;
    int numRowsAvailable;
    int numRowsVisible;

    Vector2 lastScrollPos;

    // Vertical layout properties
    float topPadding;
    float cellHeight;
    
    int currentTopIndex = 0;
    int currentFirstRow = 1;

    bool initialized = false;
    public UI_Inventory UIInventory { private get; set; } // UI for this scrollview

    void OnEnable()
    {
        if (!initialized)
            StartCoroutine(WaitForNextFrame());
    }

    void OnDisable()
    {
        initialized = false;
    }

    void OnDestroy()
    {
        customRowPrefab = null;
        scrollRect = null;
        contentTransform = null;
        verticalLayoutGrp = null;

        contentRowList = null;
}

    IEnumerator WaitForNextFrame()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 1;
        initialized = true;
    }

    public void Clear()
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

    public void InitScrollView(UI_Inventory uiInventory)
    {
        currentTopIndex = 0;
        currentFirstRow = 1;

        scrollRect.verticalNormalizedPosition = 1;

        contentRowList = new List<GameObject>();

        visibleScrollHeight = gameObject.GetComponent<RectTransform>().rect.height;
        rowHeight = customRowPrefab.GetComponent<LayoutElement>().preferredHeight;

        UIInventory = uiInventory;

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnUpdateScrollView);
    }

    public void PopulateRows()
    {
        Clear();

        currentTopIndex = 0;
        currentFirstRow = 1;
        scrollRect.verticalNormalizedPosition = 1;
        numRowsAvailable = 0;

        int itemCount = UIInventory.DisplayItemList.Count;
        if (itemCount > 0)
        {
            int rowCount = Mathf.CeilToInt((float)itemCount / cellsPerRow);
            InitRows(rowCount);
            UpdateVisibleRows();
        }
    }

    public void InitRows(int rowCount)
    {
        maxRows = rowCount;

        topPadding = verticalLayoutGrp.padding.top;
        cellHeight = verticalLayoutGrp.spacing + rowHeight;

        numRowsVisible = (int)Math.Ceiling(visibleScrollHeight / cellHeight);
        int paddedRows = numRowsVisible + paddingRows;
        int actualRows = maxRows >= paddedRows ? paddedRows : maxRows;
        numRowsAvailable = actualRows;

        // Create parent rows
        for (int i = 0; i < maxRows; ++i)
        {
            GameObject row = Instantiate(customRowPrefab);
            row.transform.SetParent(contentTransform, false);
            contentRowList.Add(row);
        }

        if (currentTopIndex + numRowsAvailable >= maxRows)
        {
            if (maxRows < numRowsVisible)
            {
                currentTopIndex = 0;
                currentFirstRow = 1;
            }
            else
            {
                currentTopIndex = maxRows - numRowsAvailable;
                if (currentTopIndex < 0)
                {
                    currentTopIndex = 0;
                }
            }
        }
    }

    public void UpdateVisibleRows()
    {
        // Update row child objects
        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            contentRowList[j].GetComponent<InventoryScrollRow>().Init(UIInventory, j, cellsPerRow);
        }
    }

    public void UpdateVisibleRowsCallback()
    {
        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            contentRowList[j].GetComponent<InventoryScrollRow>().UpdateCallback(UIInventory, j*cellsPerRow);
        }
    }

    void OnUpdateScrollView(Vector2 scrollPos)
    {
        float posY = contentTransform.localPosition.y;

        posY -= topPadding;
        bool scrollUp = lastScrollPos.y > scrollPos.y;

        int newFirst = Mathf.Clamp(TopRowSeen(posY), 1, maxRows - numRowsAvailable + 1);
        int newLast = newFirst + numRowsVisible - 1;

        if (currentFirstRow != newFirst)
        {
            if (scrollUp && newFirst > 1 && newLast < maxRows && (currentTopIndex + numRowsAvailable) < contentRowList.Count)
            {
                int diff = newFirst - currentFirstRow;
                for (int i = 0; i < diff; ++i)
                {
                    int newIndex = currentTopIndex + numRowsAvailable;

                    contentRowList[currentTopIndex].GetComponent<InventoryScrollRow>().Clear();
                    contentRowList[newIndex].GetComponent<InventoryScrollRow>().Init(UIInventory, newIndex, cellsPerRow);

                    ++currentTopIndex;
                }
            }
            else if (!scrollUp && newLast < maxRows - 1 && currentTopIndex > 0)
            {
                int diff = currentFirstRow - newFirst;
                for (int i = 0; i < diff; ++i)
                {
                    int newIndex = currentTopIndex - 1;

                    contentRowList[newIndex+numRowsAvailable].GetComponent<InventoryScrollRow>().Clear();
                    contentRowList[newIndex].GetComponent<InventoryScrollRow>().Init(UIInventory, newIndex, cellsPerRow);

                    --currentTopIndex;
                }
            }
            currentFirstRow = newFirst;
        }
        lastScrollPos = scrollPos;
    }

    int TopRowSeen(float posY)
    {
        var hiddenGrids = Math.Floor(posY / cellHeight);
        return (int)hiddenGrids + 1;
    }
}
