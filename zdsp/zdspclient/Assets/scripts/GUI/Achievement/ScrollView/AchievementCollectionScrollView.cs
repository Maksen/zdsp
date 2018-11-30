using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class AchievementCollectionScrollView : MonoBehaviour
{
    [SerializeField] int paddingRows = 2;
    [SerializeField] int cellsPerRow = 4;
    [SerializeField] GameObject rowPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform contentTransform;
    [SerializeField] VerticalLayoutGroup verticalLayout;
    [SerializeField] ToggleGroup toggleGroup;

    private List<GameObject> contentRowList = new List<GameObject>();

    //scroll rect properties
    private int maxRows;

    private float visibleHeight;
    private int numRowsAvailable;
    private int numRowsVisible;

    private Vector2 lastScrollPos;

    private float topPadding;
    private float iconHeight;
    private float iconWidth;
    private float cellHeight;

    private int currentTopIndex;
    private int currentFirstRow;
    private bool initialized = false;

    private List<CollectionInfo> dataList;
    private UnityAction<bool, CollectionObjective> OnClickDataCallback;
    private int selectedIndex;

    public void Clear()
    {
        for (int i = 0; i < contentRowList.Count; ++i)
        {
            GameObject rowData = contentRowList[i];
            if (rowData != null)
            {
                Achievement_CollectionRowData colRowData = rowData.GetComponent<Achievement_CollectionRowData>();
                colRowData.ClearRow();
                Destroy(rowData);
                rowData = null;
            }
        }
        contentRowList.Clear();
    }

    void OnDisable()
    {
        initialized = false;
    }

    public void InitScrollView(UnityAction<bool, CollectionObjective> callback)
    {
        visibleHeight = GetComponent<RectTransform>().rect.height;

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnUpdateScroll);

        OnClickDataCallback = callback;
    }

    public void Populate(List<CollectionInfo> collectionList, int selected)
    {
        Clear();

        currentTopIndex = 0;
        currentFirstRow = 1;
        scrollRect.verticalNormalizedPosition = 1;

        int maxrows = Mathf.CeilToInt((float)collectionList.Count / cellsPerRow);
        InitRows(maxrows);
        dataList = collectionList;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            int realStart = j * cellsPerRow;
            //int realEnd = (j + numRowsAvailable) * cellsPerRow;

            var currentRow = contentRowList[i];
            for (int c = 0; c < cellsPerRow; ++c)
            {
                Achievement_CollectionRowData rowData = currentRow.GetComponent<Achievement_CollectionRowData>();
                int realIdx = realStart + c;
                if (realIdx >= dataList.Count)
                    break;

                CollectionInfo info = dataList[realIdx];
                rowData.AddData(info, OnClickDataCallback);
                if (realIdx == selected)
                    rowData.GetComponent<Achievement_CollectionRowData>().SelectChild(c);
            }
        }

        if (!initialized)
            StartCoroutine(WaitForFrame());
    }

    IEnumerator WaitForFrame()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 1;
        initialized = true;
    }

    public void Refresh(List<CollectionInfo> collectionList)
    {
        dataList = collectionList;

        for (int i = 0, j = currentTopIndex; i < numRowsAvailable; ++i, ++j)
        {
            int realStart = j * cellsPerRow;
            //int realEnd = (j + numRowsAvailable) * cellsPerRow;

            var currentRow = contentRowList[i];
            for (int c = 0; c < cellsPerRow; ++c)
            {
                Achievement_CollectionRowData rowData = currentRow.GetComponent<Achievement_CollectionRowData>();
                int realIdx = realStart + c;
                if (realIdx >= dataList.Count)
                    break;

                CollectionInfo info = dataList[realIdx];
                rowData.UpdateData(c, info);
            }
        }
    }

    public void InitRows(int maxrows)
    {
        maxRows = maxrows;

        topPadding = verticalLayout.padding.top;
        LayoutElement dataLayoutElement = rowPrefab.GetComponent<LayoutElement>();
        iconHeight = dataLayoutElement.preferredHeight;
        iconWidth = dataLayoutElement.preferredWidth;
        cellHeight = verticalLayout.spacing + iconHeight;

        numRowsVisible = Mathf.CeilToInt(visibleHeight / cellHeight);
        int paddedRows = numRowsVisible + paddingRows;
        numRowsAvailable = maxRows >= paddedRows ? paddedRows : maxRows;

        //create parent rows
        for (int i = 1; i <= maxRows; ++i)
        {
            GameObject row = Instantiate(rowPrefab);
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

        //Init first numRowsAvailable rows
        for (int i = currentTopIndex; i < currentTopIndex + numRowsAvailable; ++i)
        {
            contentRowList[i].GetComponent<Achievement_CollectionRowData>().Init(cellsPerRow, toggleGroup);
        }
    }

    void OnUpdateScroll(Vector2 scrollpos)
    {
        float posY = contentTransform.localPosition.y;

        posY -= topPadding;
        bool scrollUp = lastScrollPos.y > scrollpos.y;

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
                    GameObject newRow = contentRowList[newIndex];

                    contentRowList[currentTopIndex].GetComponent<Achievement_CollectionRowData>().TransferChildrenTo(newRow);
                    RefreshNewRow(newRow, newIndex);

                    currentTopIndex++;
                }
            }
            else if (!scrollUp && newLast < maxRows - 1 && currentTopIndex > 0)
            {
                int diff = currentFirstRow - newFirst;
                for (int i = 0; i < diff; ++i)
                {
                    int newIndex = currentTopIndex - 1;
                    GameObject newRow = contentRowList[newIndex];

                    contentRowList[newIndex + numRowsAvailable].GetComponent<Achievement_CollectionRowData>().TransferChildrenTo(newRow);
                    RefreshNewRow(newRow, newIndex);

                    currentTopIndex--;
                }
            }

            currentFirstRow = newFirst;
        }

        lastScrollPos = scrollpos;
    }

    private int TopRowSeen(float posY)
    {
        int hiddenGrids = Mathf.FloorToInt(posY / cellHeight);
        return hiddenGrids + 1;
    }

    private void RefreshNewRow(GameObject newRow, int newIndex)
    {
        if (newIndex < 0)
            return;

        for (int i = 0; i < cellsPerRow; ++i)
        {
            int realStart = newIndex * cellsPerRow;
            int realIdx = realStart + i;

            if (realIdx >= dataList.Count)
                break;

            CollectionInfo info = dataList[realIdx];
            newRow.GetComponent<Achievement_CollectionRowData>().AddData(info, OnClickDataCallback);
            if (realIdx == selectedIndex)
                newRow.GetComponent<Achievement_CollectionRowData>().SelectChild(i);
        }
    }

    public void SelectFirstDataItem()
    {
        if (contentRowList.Count > 0)
        {
            contentRowList[0].GetComponent<Achievement_CollectionRowData>().SelectChild(0);
        }
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;
    }
}